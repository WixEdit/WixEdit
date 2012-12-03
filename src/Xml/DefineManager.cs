using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

namespace WixEdit.Xml
{
	public class DefineManager
	{
		WixFiles wixFiles;
		XmlDocument wxsDocument;
		Dictionary<String, String> dVarDefines;

		public DefineManager(WixFiles wixFiles, XmlDocument wxsDocument)
		{
			this.wixFiles = wixFiles;
			this.wxsDocument = wxsDocument;
			LoadDefines(wixFiles.WxsFile);
		}

		public bool HasDefines
		{
			get
			{
				return (dVarDefines != null) && (dVarDefines.Count > 0);
			}
		}

		private static String[] splitKeyValue(String data)
		{
			data = data.Trim('"', '\r', '\n', ' ');
			if (!String.IsNullOrEmpty(data))
			{
				return data.Split(new char[] { '=' }, 2);
			}
			return null;
		}

		private IEnumerable<XmlNode> GetAllChildNodes(XmlNode n)
		{
			foreach (XmlNode nChild in n.ChildNodes)
			{
				yield return nChild;
				foreach (XmlNode subChild in GetAllChildNodes(nChild))
				{
					yield return subChild;
				}
			}
		}

		private void LoadDefines(FileInfo file)
		{
			dVarDefines = new Dictionary<String, String>();

			int iSkipToEndIf = 0;
			int iSkipElse = 0;

			List<XmlNode> nodesToRemove = new List<XmlNode>();

			try
			{
				// Verify valid xml
				foreach (XmlNode node in GetAllChildNodes(wxsDocument))
				{
					XmlProcessingInstruction define = node as XmlProcessingInstruction;
					if (define != null)
					{
						if (define.Name == "endif")
						{
							//reduce level
							if (iSkipToEndIf > 0)
							{
								iSkipToEndIf--;
							}
							if (iSkipElse > 0)
							{
								iSkipElse--;
							}
							continue;
						}
						else if (define.Name == "else")
						{
							if (iSkipElse > 0)
							{
								iSkipElse--;
								continue;
							}
						}

						String[] a = splitKeyValue(define.Data);
						if (a == null)
						{
							continue;
						}

						if (a.Length >= 1)
						{
							//check
							if (define.Name == "ifdef")
							{
								if (iSkipToEndIf > 0)
								{
									iSkipToEndIf++;
									iSkipElse++;
								}
								else if (!dVarDefines.ContainsKey(a[0]))
								{
									iSkipToEndIf++;
								}
							}
							//check
							else if (define.Name == "ifndef")
							{
								if (iSkipToEndIf > 0)
								{
									iSkipToEndIf++;
									iSkipElse++;
								}
								else if (dVarDefines.ContainsKey(a[0]))
								{
									iSkipToEndIf++;
								}
							}
						}

						if (iSkipToEndIf > 0)
						{
							continue;
						}

						if (define.Name == "undef")
						{
							dVarDefines.Remove(a[0]);
						}
						else if (define.Name == "define")
						{
							if (a.Length == 2)
							{
								String key = a[0];
								String value = a[1].Trim('\"');
								//add mapping and apply defines
								dVarDefines[key] = ApplyDefines(value, file != null ? file.FullName : null);
							}
							//length of 1 denotes conditional define
							else if (a.Length == 1)
							{
								dVarDefines[a[0]] = null;
							}
						}
					}
					else
					{
						//remove nodes in skipped blocks later on
						if (iSkipToEndIf > 0)
						{
							nodesToRemove.Add(node);
						}
					}
				}
				//remove nodes, which belong to skipped blocks
				foreach (XmlNode n in nodesToRemove)
				{
					n.ParentNode.RemoveChild(n);
				}
			}
			catch (Exception ex)
			{
				throw new WixEditException("Loading of defines failed!!!", ex);
			}
		}

		public String ApplyDefines(String data, String filename)
		{
			//check for variable signature
			if (!String.IsNullOrEmpty(data) && data.Contains("$("))
			{
				if (filename != null)
				{
					String path = Path.GetDirectoryName(filename) + "\\";
					data = data.Replace("$(sys.SOURCEFILEDIR)", path);
				}
				//TODO: optimize this!
				foreach (String key in dVarDefines.Keys)
				{
					String definition = "$(var." + key + ")";
					String value = dVarDefines[key];
					data = data.Replace(definition, value);
				}
			}
			return data;
		}

	}
}

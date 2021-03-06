using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Verse
{
	public static class DirectXmlSaver
	{
		public static bool IsSimpleTextType(Type type)
		{
			return type == typeof(float) || type == typeof(int) || type == typeof(bool) || type == typeof(string) || type.IsEnum;
		}

		public static void SaveDataObject(object obj, string filePath)
		{
			try
			{
				XDocument xDocument = new XDocument();
				XElement content = DirectXmlSaver.XElementFromObject(obj, obj.GetType());
				xDocument.Add(content);
				xDocument.Save(filePath);
			}
			catch (Exception ex)
			{
				GenUI.ErrorDialog("ProblemSavingFile".Translate(filePath, ex.ToString()));
				Log.Error("Exception saving data object " + obj + ": " + ex);
			}
		}

		public static XElement XElementFromObject(object obj, Type expectedClass)
		{
			return DirectXmlSaver.XElementFromObject(obj, expectedClass, expectedClass.Name, null, false);
		}

		public static XElement XElementFromObject(object obj, Type expectedType, string nodeName, FieldInfo owningField = null, bool saveDefsAsRefs = false)
		{
			DefaultValueAttribute defaultValueAttribute = default(DefaultValueAttribute);
			if (owningField != null && ((MemberInfo)owningField).TryGetAttribute<DefaultValueAttribute>(out defaultValueAttribute) && defaultValueAttribute.ObjIsDefault(obj))
			{
				return null;
			}
			if (obj == null)
			{
				XElement xElement = new XElement(nodeName);
				xElement.SetAttributeValue("IsNull", "True");
				return xElement;
			}
			Type type = obj.GetType();
			XElement xElement2 = new XElement(nodeName);
			if (DirectXmlSaver.IsSimpleTextType(type))
			{
				xElement2.Add(new XText(obj.ToString()));
				goto IL_02fd;
			}
			if (saveDefsAsRefs && typeof(Def).IsAssignableFrom(type))
			{
				string defName = ((Def)obj).defName;
				xElement2.Add(new XText(defName));
				goto IL_02fd;
			}
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
			{
				Type expectedType2 = type.GetGenericArguments()[0];
				int num = (int)type.GetProperty("Count").GetValue(obj, null);
				for (int i = 0; i < num; i++)
				{
					object[] index = new object[1]
					{
						i
					};
					object value = type.GetProperty("Item").GetValue(obj, index);
					XNode content = DirectXmlSaver.XElementFromObject(value, expectedType2, "li", null, true);
					xElement2.Add(content);
				}
				goto IL_02fd;
			}
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<, >))
			{
				Type expectedType3 = type.GetGenericArguments()[0];
				Type expectedType4 = type.GetGenericArguments()[1];
				IEnumerator enumerator = (obj as IEnumerable).GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						object current = enumerator.Current;
						object value2 = current.GetType().GetProperty("Key").GetValue(current, null);
						object value3 = current.GetType().GetProperty("Value").GetValue(current, null);
						XElement xElement3 = new XElement("li");
						xElement3.Add(DirectXmlSaver.XElementFromObject(value2, expectedType3, "key", null, true));
						xElement3.Add(DirectXmlSaver.XElementFromObject(value3, expectedType4, "value", null, true));
						xElement2.Add(xElement3);
					}
					return xElement2;
				}
				finally
				{
					IDisposable disposable;
					if ((disposable = (enumerator as IDisposable)) != null)
					{
						disposable.Dispose();
					}
				}
			}
			if (type != expectedType)
			{
				XAttribute content2 = new XAttribute("Class", GenTypes.GetTypeNameWithoutIgnoredNamespaces(obj.GetType()));
				xElement2.Add(content2);
			}
			foreach (FieldInfo item in from f in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			orderby f.MetadataToken
			select f)
			{
				try
				{
					XElement xElement4 = DirectXmlSaver.XElementFromField(item, obj);
					if (xElement4 != null)
					{
						xElement2.Add(xElement4);
					}
				}
				catch
				{
					throw;
				}
			}
			return xElement2;
			IL_02fd:
			return xElement2;
		}

		private static XElement XElementFromField(FieldInfo fi, object owningObj)
		{
			if (Attribute.IsDefined(fi, typeof(UnsavedAttribute)))
			{
				return null;
			}
			object value = fi.GetValue(owningObj);
			return DirectXmlSaver.XElementFromObject(value, fi.FieldType, fi.Name, fi, false);
		}
	}
}

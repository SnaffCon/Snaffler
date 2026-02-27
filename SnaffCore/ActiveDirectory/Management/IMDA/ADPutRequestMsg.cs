using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Xml;

namespace Microsoft.ActiveDirectory.Management.IMDA
{
	// Token: 0x0200030A RID: 778
	internal class ADPutRequestMsg : AdimdaRequestMsg
	{
		// Token: 0x06001ACA RID: 6858 RVA: 0x00052E99 File Offset: 0x00051099
		private ADPutRequestMsg(string instance, string objectReference, IList<DirectoryControl> controls)
			: base(instance, objectReference)
		{
			this._controls = controls;
		}

		// Token: 0x06001ACB RID: 6859 RVA: 0x00052EAA File Offset: 0x000510AA
		public ADPutRequestMsg(string instance, string objectReference, IList<DirectoryControl> controls, IList<DirectoryAttributeModification> attributeModifications)
			: this(instance, objectReference, controls)
		{
			this._attributeMods = attributeModifications;
		}

		// Token: 0x06001ACC RID: 6860 RVA: 0x00052EBD File Offset: 0x000510BD
		public ADPutRequestMsg(string instance, string objectReference, IList<DirectoryControl> controls, string relativeDistinguishedName)
			: this(instance, objectReference, controls)
		{
			this._relativeDistinguishedName = relativeDistinguishedName;
		}

		// Token: 0x06001ACD RID: 6861 RVA: 0x00052ED0 File Offset: 0x000510D0
		public ADPutRequestMsg(string instance, string objectReference, IList<DirectoryControl> controls, string relativeDistinguishedName, string parent)
			: this(instance, objectReference, controls)
		{
			this._relativeDistinguishedName = relativeDistinguishedName;
			this._parent = parent;
		}

		// Token: 0x06001ACE RID: 6862 RVA: 0x00052EEB File Offset: 0x000510EB
		public ADPutRequestMsg(string instance, string objectReference, IList<DirectoryControl> controls, string relativeDistinguishedName, string parent, IList<DirectoryAttributeModification> attributeModifications)
			: this(instance, objectReference, controls)
		{
			this._relativeDistinguishedName = relativeDistinguishedName;
			this._parent = parent;
			this._attributeMods = attributeModifications;
		}

		// Token: 0x17000916 RID: 2326
		// (get) Token: 0x06001ACF RID: 6863 RVA: 0x00052F0E File Offset: 0x0005110E
		public override string Action
		{
			get
			{
				return "http://schemas.xmlsoap.org/ws/2004/09/transfer/Put";
			}
		}

		// Token: 0x06001AD0 RID: 6864 RVA: 0x00052F18 File Offset: 0x00051118
		protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
		{
			base.OnWriteBodyContents(writer);
			writer.WriteStartElement("ModifyRequest", "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess");
			writer.WriteAttributeString("Dialect", "http://schemas.microsoft.com/2008/1/ActiveDirectory/Dialect/XPath-Level-1");
			if (this._attributeMods != null)
			{
				foreach (DirectoryAttributeModification directoryAttributeModification in this._attributeMods)
				{
					AttributeTypeAndValueSerializer.Serialize(writer, directoryAttributeModification);
				}
			}
			if (this._parent != null)
			{
				AttributeTypeAndValueSerializer.Serialize(writer, ChangeOperation.Replace, "http://schemas.microsoft.com/2008/1/ActiveDirectory", "container-hierarchy-parent", this._parent);
			}
			if (this._relativeDistinguishedName != null)
			{
				AttributeTypeAndValueSerializer.Serialize(writer, ChangeOperation.Replace, "http://schemas.microsoft.com/2008/1/ActiveDirectory", "relativeDistinguishedName", this._relativeDistinguishedName);
			}
			DirectoryControlSerializer.Serialize(writer, this._controls);
			writer.WriteEndElement();
		}

		// Token: 0x04000C7F RID: 3199
		private IList<DirectoryControl> _controls;

		// Token: 0x04000C80 RID: 3200
		private IList<DirectoryAttributeModification> _attributeMods;

		// Token: 0x04000C81 RID: 3201
		private string _relativeDistinguishedName;

		// Token: 0x04000C82 RID: 3202
		private string _parent;
	}
}

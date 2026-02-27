using System;
using System.CodeDom.Compiler;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.ActiveDirectory.WebServices.Proxy
{
	// Token: 0x0200004C RID: 76
	[GeneratedCode("System.ServiceModel", "3.0.0.0")]
	[ServiceContract(Namespace = "http://schemas.xmlsoap.org/ws/2004/09/transfer", ConfigurationName = "Resource", SessionMode = SessionMode.Required)]
	internal interface Resource
	{
		// Token: 0x06000180 RID: 384
		[OperationContract(Action = "http://schemas.xmlsoap.org/ws/2004/09/transfer/Get", ReplyAction = "http://schemas.xmlsoap.org/ws/2004/09/transfer/GetResponse")]
		[FaultContract(typeof(FaultDetail), Action = "http://schemas.dmtf.org/wbem/wsman/1/wsman/fault", Name = "AccessDenied", Namespace = "http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd")]
		[FaultContract(typeof(FaultDetail1), Action = "http://schemas.dmtf.org/wbem/wsman/1/wsman/fault", Name = "EncodingLimit", Namespace = "http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd")]
		[FaultContract(typeof(FaultDetail), Action = "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess/fault", Name = "UnwillingToPerform", Namespace = "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess")]
		[FaultContract(typeof(FragmentDialect), Action = "http://schemas.dmtf.org/wbem/wsman/1/wsman/fault", Name = "FragmentDialectNotSupported", Namespace = "http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd")]
		[FaultContract(typeof(FaultDetail), Action = "http://schemas.xmlsoap.org/ws/2004/08/addressing/fault", Name = "EndpointUnavailable", Namespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing")]
		[FaultContract(typeof(AttributeTypeNotValid), Action = "http://schemas.dmtf.org/wbem/wsman/1/wsman/fault", Name = "CannotProcessFilter", Namespace = "http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd")]
		[FaultContract(typeof(FaultDetail), Action = "http://schemas.dmtf.org/wbem/wsman/1/wsman/fault", Name = "SchemaValidationError", Namespace = "http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd")]
		[FaultContract(typeof(FaultDetail), Action = "http://schemas.xmlsoap.org/ws/2004/08/addressing/fault", Name = "DestinationUnreachable", Namespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing")]
		[XmlSerializerFormat(SupportFaults = true)]
		Message Get(Message request);

		// Token: 0x06000181 RID: 385
		[OperationContract(Action = "http://schemas.xmlsoap.org/ws/2004/09/transfer/Put", ReplyAction = "http://schemas.xmlsoap.org/ws/2004/09/transfer/PutResponse")]
		[FaultContract(typeof(AttributeTypeNotValid), Action = "http://schemas.dmtf.org/wbem/wsman/1/wsman/fault", Name = "CannotProcessFilter", Namespace = "http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd")]
		[FaultContract(typeof(FaultDetail), Action = "http://schemas.dmtf.org/wbem/wsman/1/wsman/fault", Name = "AccessDenied", Namespace = "http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd")]
		[FaultContract(typeof(FaultDetail), Action = "http://schemas.xmlsoap.org/ws/2004/08/addressing/fault", Name = "DestinationUnreachable", Namespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing")]
		[FaultContract(typeof(FaultDetail1), Action = "http://schemas.dmtf.org/wbem/wsman/1/wsman/fault", Name = "EncodingLimit", Namespace = "http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd")]
		[FaultContract(typeof(FaultDetail), Action = "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess/fault", Name = "UnwillingToPerform", Namespace = "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess")]
		[FaultContract(typeof(FragmentDialect), Action = "http://schemas.dmtf.org/wbem/wsman/1/wsman/fault", Name = "FragmentDialectNotSupported", Namespace = "http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd")]
		[FaultContract(typeof(FaultDetail), Action = "http://schemas.xmlsoap.org/ws/2004/08/addressing/fault", Name = "EndpointUnavailable", Namespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing")]
		[FaultContract(typeof(FaultDetail), Action = "http://schemas.xmlsoap.org/ws/2004/09/transfer/fault", Name = "InvalidRepresentation")]
		[FaultContract(typeof(FaultDetail), Action = "http://schemas.dmtf.org/wbem/wsman/1/wsman/fault", Name = "SchemaValidationError", Namespace = "http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd")]
		[FaultContract(typeof(FaultDetail), Action = "http://schemas.xmlsoap.org/ws/2004/08/addressing/fault", Name = "ActionNotSupportedFault", Namespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing")]
		[XmlSerializerFormat(SupportFaults = true)]
		Message Put(Message request);

		// Token: 0x06000182 RID: 386
		[OperationContract(Action = "http://schemas.xmlsoap.org/ws/2004/09/transfer/Delete", ReplyAction = "http://schemas.xmlsoap.org/ws/2004/09/transfer/DeleteResponse")]
		[FaultContract(typeof(FaultDetail), Action = "http://schemas.xmlsoap.org/ws/2004/08/addressing/fault", Name = "EndpointUnavailable", Namespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing")]
		[FaultContract(typeof(FaultDetail), Action = "http://schemas.xmlsoap.org/ws/2004/08/addressing/fault", Name = "DestinationUnreachable", Namespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing")]
		[FaultContract(typeof(FaultDetail), Action = "http://schemas.dmtf.org/wbem/wsman/1/wsman/fault", Name = "SchemaValidationError", Namespace = "http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd")]
		[FaultContract(typeof(FaultDetail), Action = "http://schemas.dmtf.org/wbem/wsman/1/wsman/fault", Name = "AccessDenied", Namespace = "http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd")]
		[FaultContract(typeof(FaultDetail), Action = "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess/fault", Name = "UnwillingToPerform", Namespace = "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess")]
		[XmlSerializerFormat(SupportFaults = true)]
		Message Delete(Message request);
	}
}

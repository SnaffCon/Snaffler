using System;
using System.CodeDom.Compiler;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.ActiveDirectory.WebServices.Proxy
{
	// Token: 0x0200004F RID: 79
	[GeneratedCode("System.ServiceModel", "3.0.0.0")]
	[ServiceContract(Namespace = "http://schemas.xmlsoap.org/ws/2004/09/transfer", ConfigurationName = "ResourceFactory", SessionMode = SessionMode.Required)]
	internal interface ResourceFactory
	{
		// Token: 0x0600018B RID: 395
		[OperationContract(Action = "http://schemas.xmlsoap.org/ws/2004/09/transfer/Create", ReplyAction = "http://schemas.xmlsoap.org/ws/2004/09/transfer/CreateResponse")]
		[FaultContract(typeof(FaultDetail1), Action = "http://schemas.dmtf.org/wbem/wsman/1/wsman/fault", Name = "EncodingLimit", Namespace = "http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd")]
		[FaultContract(typeof(FaultDetail), Action = "http://schemas.xmlsoap.org/ws/2004/08/addressing/fault", Name = "DestinationUnreachable", Namespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing")]
		[FaultContract(typeof(FaultDetail), Action = "http://schemas.dmtf.org/wbem/wsman/1/wsman/fault", Name = "AlreadyExists", Namespace = "http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd")]
		[FaultContract(typeof(FaultDetail), Action = "http://schemas.dmtf.org/wbem/wsman/1/wsman/fault", Name = "SchemaValidationError", Namespace = "http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd")]
		[FaultContract(typeof(AttributeTypeNotValid), Action = "http://schemas.dmtf.org/wbem/wsman/1/wsman/fault", Name = "CannotProcessFilter", Namespace = "http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd")]
		[FaultContract(typeof(FaultDetail), Action = "http://schemas.xmlsoap.org/ws/2004/08/addressing/fault", Name = "EndpointUnavailable", Namespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing")]
		[FaultContract(typeof(FaultDetail), Action = "http://schemas.xmlsoap.org/ws/2004/08/addressing/fault", Name = "ActionNotSupportedFault", Namespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing")]
		[FaultContract(typeof(FaultDetail), Action = "http://schemas.xmlsoap.org/ws/2004/09/transfer/fault", Name = "InvalidRepresentation")]
		[FaultContract(typeof(FragmentDialect), Action = "http://schemas.dmtf.org/wbem/wsman/1/wsman/fault", Name = "FragmentDialectNotSupported", Namespace = "http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd")]
		[FaultContract(typeof(FaultDetail), Action = "http://schemas.dmtf.org/wbem/wsman/1/wsman/fault", Name = "AccessDenied", Namespace = "http://schemas.dmtf.org/wbem/wsman/1/wsman.xsd")]
		[FaultContract(typeof(FaultDetail), Action = "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess/fault", Name = "UnwillingToPerform", Namespace = "http://schemas.microsoft.com/2006/11/IdentityManagement/DirectoryAccess")]
		[XmlSerializerFormat(SupportFaults = true)]
		Message Create(Message request);
	}
}

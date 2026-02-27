using System;
using System.CodeDom.Compiler;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.ActiveDirectory.WebServices.Proxy
{
	// Token: 0x02000052 RID: 82
	[GeneratedCode("System.ServiceModel", "3.0.0.0")]
	[ServiceContract(Namespace = "http://schemas.xmlsoap.org/ws/2004/09/enumeration", ConfigurationName = "Search", SessionMode = SessionMode.Required)]
	internal interface Search
	{
		// Token: 0x06000192 RID: 402
		[OperationContract(Action = "http://schemas.xmlsoap.org/ws/2004/09/enumeration/Enumerate", ReplyAction = "http://schemas.xmlsoap.org/ws/2004/09/enumeration/EnumerateResponse")]
		[FaultContract(typeof(EnumerateFault), Action = "http://schemas.microsoft.com/2008/1/ActiveDirectory/Data/fault", Name = "InvalidSortKey", Namespace = "http://schemas.microsoft.com/2008/1/ActiveDirectory")]
		[FaultContract(typeof(EnumerateFault), Action = "http://schemas.microsoft.com/2008/1/ActiveDirectory/Data/fault", Name = "InvalidPropertyFault", Namespace = "http://schemas.microsoft.com/2008/1/ActiveDirectory")]
		[FaultContract(typeof(SupportedSelectOrSortDialect), Action = "http://schemas.microsoft.com/2008/1/ActiveDirectory/Data/fault", Name = "UnsupportedSelectOrSortDialectFault", Namespace = "http://schemas.microsoft.com/2008/1/ActiveDirectory")]
		[FaultContract(typeof(EnumerateFault), Action = "http://schemas.microsoft.com/2008/1/ActiveDirectory/Data/fault", Name = "EnumerationContextLimitExceeded", Namespace = "http://schemas.microsoft.com/2008/1/ActiveDirectory")]
		[FaultContract(typeof(EnumerateFault), Action = "http://schemas.xmlsoap.org/ws/2004/09/enumeration/fault", Name = "CannotProcessFilter")]
		[FaultContract(typeof(SupportedDialect), Action = "http://schemas.xmlsoap.org/ws/2004/09/enumeration/fault", Name = "FilterDialectRequestedUnavailable")]
		[FaultContract(typeof(EnumerateFault), Action = "http://schemas.xmlsoap.org/ws/2004/09/enumeration/fault", Name = "InvalidExpirationTime")]
		[FaultContract(typeof(FaultDetail), Action = "http://schemas.xmlsoap.org/ws/2004/08/addressing/fault", Name = "DestinationUnreachable", Namespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing")]
		[FaultContract(typeof(FaultDetail), Action = "http://schemas.xmlsoap.org/ws/2004/08/addressing/fault", Name = "EndpointUnavailable", Namespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing")]
		[XmlSerializerFormat(SupportFaults = true)]
		Message Enumerate(Message request);

		// Token: 0x06000193 RID: 403
		[OperationContract(Action = "http://schemas.xmlsoap.org/ws/2004/09/enumeration/Pull", ReplyAction = "http://schemas.xmlsoap.org/ws/2004/09/enumeration/PullResponse")]
		[FaultContract(typeof(PullFault), Action = "http://schemas.xmlsoap.org/ws/2004/09/enumeration/fault", Name = "TimedOut")]
		[FaultContract(typeof(FaultDetail), Action = "http://schemas.xmlsoap.org/ws/2004/08/addressing/fault", Name = "EndpointUnavailable", Namespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing")]
		[FaultContract(typeof(PullFault), Action = "http://schemas.microsoft.com/2008/1/ActiveDirectory/Data/fault", Name = "MaxCharsNotSupported", Namespace = "http://schemas.microsoft.com/2008/1/ActiveDirectory")]
		[FaultContract(typeof(FaultDetail), Action = "http://schemas.xmlsoap.org/ws/2004/08/addressing/fault", Name = "DestinationUnreachable", Namespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing")]
		[FaultContract(typeof(FaultDetail), Action = "http://schemas.xmlsoap.org/ws/2004/09/enumeration/fault", Name = "InvalidEnumerationContext")]
		[FaultContract(typeof(PullFault), Action = "http://schemas.microsoft.com/2008/1/ActiveDirectory/Data/fault", Name = "MaxTimeExceedsLimit", Namespace = "http://schemas.microsoft.com/2008/1/ActiveDirectory")]
		[XmlSerializerFormat(SupportFaults = true)]
		Message Pull(Message request);

		// Token: 0x06000194 RID: 404
		[OperationContract(Action = "http://schemas.xmlsoap.org/ws/2004/09/enumeration/Renew", ReplyAction = "http://schemas.xmlsoap.org/ws/2004/09/enumeration/RenewResponse")]
		[FaultContract(typeof(FaultDetail), Action = "http://schemas.xmlsoap.org/ws/2004/08/addressing/fault", Name = "EndpointUnavailable", Namespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing")]
		[FaultContract(typeof(RenewFault), Action = "http://schemas.xmlsoap.org/ws/2004/09/enumeration/fault", Name = "UnableToRenew")]
		[FaultContract(typeof(FaultDetail), Action = "http://schemas.xmlsoap.org/ws/2004/09/enumeration/fault", Name = "InvalidEnumerationContext")]
		[XmlSerializerFormat(SupportFaults = true)]
		Message Renew(Message request);

		// Token: 0x06000195 RID: 405
		[OperationContract(Action = "http://schemas.xmlsoap.org/ws/2004/09/enumeration/GetStatus", ReplyAction = "http://schemas.xmlsoap.org/ws/2004/09/enumeration/GetStatusResponse")]
		[FaultContract(typeof(FaultDetail), Action = "http://schemas.xmlsoap.org/ws/2004/08/addressing/fault", Name = "EndpointUnavailable", Namespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing")]
		[FaultContract(typeof(FaultDetail), Action = "http://schemas.xmlsoap.org/ws/2004/09/enumeration/fault", Name = "InvalidEnumerationContext")]
		[XmlSerializerFormat(SupportFaults = true)]
		Message GetStatus(Message request);

		// Token: 0x06000196 RID: 406
		[OperationContract(Action = "http://schemas.xmlsoap.org/ws/2004/09/enumeration/Release", ReplyAction = "http://schemas.xmlsoap.org/ws/2004/09/enumeration/ReleaseResponse")]
		[FaultContract(typeof(FaultDetail), Action = "http://schemas.xmlsoap.org/ws/2004/08/addressing/fault", Name = "EndpointUnavailable", Namespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing")]
		[XmlSerializerFormat(SupportFaults = true)]
		Message Release(Message request);
	}
}

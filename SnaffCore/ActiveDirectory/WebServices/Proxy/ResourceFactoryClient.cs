using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.ActiveDirectory.WebServices.Proxy
{
	// Token: 0x02000051 RID: 81
	[DebuggerStepThrough]
	[GeneratedCode("System.ServiceModel", "3.0.0.0")]
	internal class ResourceFactoryClient : ClientBase<ResourceFactory>, ResourceFactory
	{
		// Token: 0x0600018C RID: 396 RVA: 0x00003108 File Offset: 0x00001308
		public ResourceFactoryClient()
		{
		}

		// Token: 0x0600018D RID: 397 RVA: 0x00003110 File Offset: 0x00001310
		public ResourceFactoryClient(string endpointConfigurationName)
			: base(endpointConfigurationName)
		{
		}

		// Token: 0x0600018E RID: 398 RVA: 0x00003119 File Offset: 0x00001319
		public ResourceFactoryClient(string endpointConfigurationName, string remoteAddress)
			: base(endpointConfigurationName, remoteAddress)
		{
		}

		// Token: 0x0600018F RID: 399 RVA: 0x00003123 File Offset: 0x00001323
		public ResourceFactoryClient(string endpointConfigurationName, EndpointAddress remoteAddress)
			: base(endpointConfigurationName, remoteAddress)
		{
		}

		// Token: 0x06000190 RID: 400 RVA: 0x0000312D File Offset: 0x0000132D
		public ResourceFactoryClient(Binding binding, EndpointAddress remoteAddress)
			: base(binding, remoteAddress)
		{
		}

		// Token: 0x06000191 RID: 401 RVA: 0x00003137 File Offset: 0x00001337
		public Message Create(Message request)
		{
			return base.Channel.Create(request);
		}
	}
}

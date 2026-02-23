using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.ActiveDirectory.WebServices.Proxy
{
	// Token: 0x02000054 RID: 84
	[DebuggerStepThrough]
	[GeneratedCode("System.ServiceModel", "3.0.0.0")]
	internal class SearchClient : ClientBase<Search>, Search
	{
		// Token: 0x06000197 RID: 407 RVA: 0x00003145 File Offset: 0x00001345
		public SearchClient()
		{
		}

		// Token: 0x06000198 RID: 408 RVA: 0x0000314D File Offset: 0x0000134D
		public SearchClient(string endpointConfigurationName)
			: base(endpointConfigurationName)
		{
		}

		// Token: 0x06000199 RID: 409 RVA: 0x00003156 File Offset: 0x00001356
		public SearchClient(string endpointConfigurationName, string remoteAddress)
			: base(endpointConfigurationName, remoteAddress)
		{
		}

		// Token: 0x0600019A RID: 410 RVA: 0x00003160 File Offset: 0x00001360
		public SearchClient(string endpointConfigurationName, EndpointAddress remoteAddress)
			: base(endpointConfigurationName, remoteAddress)
		{
		}

		// Token: 0x0600019B RID: 411 RVA: 0x0000316A File Offset: 0x0000136A
		public SearchClient(Binding binding, EndpointAddress remoteAddress)
			: base(binding, remoteAddress)
		{
		}

		// Token: 0x0600019C RID: 412 RVA: 0x00003174 File Offset: 0x00001374
		public Message Enumerate(Message request)
		{
			return base.Channel.Enumerate(request);
		}

		// Token: 0x0600019D RID: 413 RVA: 0x00003182 File Offset: 0x00001382
		public Message Pull(Message request)
		{
			return base.Channel.Pull(request);
		}

		// Token: 0x0600019E RID: 414 RVA: 0x00003190 File Offset: 0x00001390
		public Message Renew(Message request)
		{
			return base.Channel.Renew(request);
		}

		// Token: 0x0600019F RID: 415 RVA: 0x0000319E File Offset: 0x0000139E
		public Message GetStatus(Message request)
		{
			return base.Channel.GetStatus(request);
		}

		// Token: 0x060001A0 RID: 416 RVA: 0x000031AC File Offset: 0x000013AC
		public Message Release(Message request)
		{
			return base.Channel.Release(request);
		}
	}
}

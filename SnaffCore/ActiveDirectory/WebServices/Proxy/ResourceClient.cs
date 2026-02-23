using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.ActiveDirectory.WebServices.Proxy
{
	// Token: 0x0200004E RID: 78
	[DebuggerStepThrough]
	[GeneratedCode("System.ServiceModel", "3.0.0.0")]
	internal class ResourceClient : ClientBase<Resource>, Resource
	{
		// Token: 0x06000183 RID: 387 RVA: 0x000030AF File Offset: 0x000012AF
		public ResourceClient()
		{
		}

		// Token: 0x06000184 RID: 388 RVA: 0x000030B7 File Offset: 0x000012B7
		public ResourceClient(string endpointConfigurationName)
			: base(endpointConfigurationName)
		{
		}

		// Token: 0x06000185 RID: 389 RVA: 0x000030C0 File Offset: 0x000012C0
		public ResourceClient(string endpointConfigurationName, string remoteAddress)
			: base(endpointConfigurationName, remoteAddress)
		{
		}

		// Token: 0x06000186 RID: 390 RVA: 0x000030CA File Offset: 0x000012CA
		public ResourceClient(string endpointConfigurationName, EndpointAddress remoteAddress)
			: base(endpointConfigurationName, remoteAddress)
		{
		}

		// Token: 0x06000187 RID: 391 RVA: 0x000030D4 File Offset: 0x000012D4
		public ResourceClient(Binding binding, EndpointAddress remoteAddress)
			: base(binding, remoteAddress)
		{
		}

		// Token: 0x06000188 RID: 392 RVA: 0x000030DE File Offset: 0x000012DE
		public Message Get(Message request)
		{
			return base.Channel.Get(request);
		}

		// Token: 0x06000189 RID: 393 RVA: 0x000030EC File Offset: 0x000012EC
		public Message Put(Message request)
		{
			return base.Channel.Put(request);
		}

		// Token: 0x0600018A RID: 394 RVA: 0x000030FA File Offset: 0x000012FA
		public Message Delete(Message request)
		{
			return base.Channel.Delete(request);
		}
	}
}

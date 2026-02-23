using System;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace Microsoft.ActiveDirectory
{
    // Token: 0x0200003C RID: 60
    internal class StringResources
    {
        // Token: 0x0600012B RID: 299 RVA: 0x00002D74 File Offset: 0x00000F74
        static StringResources()
        {
            ResourceManager resourceManager = new ResourceManager("Microsoft.ActiveDirectory.Management", Assembly.GetExecutingAssembly());
            Type typeFromHandle = typeof(StringResources);
            foreach (MemberInfo memberInfo in typeFromHandle.GetMembers(BindingFlags.Static | BindingFlags.Public))
            {
                typeFromHandle.InvokeMember(memberInfo.Name, BindingFlags.SetField, null, null, new object[] { resourceManager.GetString(memberInfo.Name, CultureInfo.CurrentUICulture) }, CultureInfo.CurrentCulture);
            }
        }

        // Token: 0x0400009E RID: 158
        public static string NoNegativeTime;

        // Token: 0x0400009F RID: 159
        public static string TimeoutError;

        // Token: 0x040000A0 RID: 160
        public static string NoNegativeSizeLimit;

        // Token: 0x040000A1 RID: 161
        public static string NoNegativePageSize;

        // Token: 0x040000A2 RID: 162
        public static string SearchSizeLimitExceeded;

        // Token: 0x040000A3 RID: 163
        public static string ExceedMax;

        // Token: 0x040000A4 RID: 164
        public static string NotSupportedSessionOption;

        // Token: 0x040000A5 RID: 165
        public static string InvalidType;

        // Token: 0x040000A6 RID: 166
        public static string InvalidYear;

        // Token: 0x040000A7 RID: 167
        public static string EmptyStringParameter;

        // Token: 0x040000A8 RID: 168
        public static string ServerDown;

        // Token: 0x040000A9 RID: 169
        public static string DefaultServerNotFound;

        // Token: 0x040000AA RID: 170
        public static string DefaultADWSServerNotFound;

        // Token: 0x040000AB RID: 171
        public static string ServerOutOfMemory;

        // Token: 0x040000AC RID: 172
        public static string NoMixedType;

        // Token: 0x040000AD RID: 173
        public static string ForestIdDoesNotMatch;

        // Token: 0x040000AE RID: 174
        public static string NotSupportedGCPort;

        // Token: 0x040000AF RID: 175
        public static string InvalidUriFormat;

        // Token: 0x040000B0 RID: 176
        public static string InvalidPartitionMustBelongToValidSet;

        // Token: 0x040000B1 RID: 177
        public static string InvalidDNMustBelongToValidPartitionSet;

        // Token: 0x040000B2 RID: 178
        public static string ADFilterOperatorNotSupported;

        // Token: 0x040000B3 RID: 179
        public static string ADFilterParsingErrorMessage;

        // Token: 0x040000B4 RID: 180
        public static string ADFilterVariableNotDefinedMessage;

        // Token: 0x040000B5 RID: 181
        public static string ADFilterExprListLessThanTwo;

        // Token: 0x040000B6 RID: 182
        public static string ADFilterPropertyNotFoundInObject;

        // Token: 0x040000B7 RID: 183
        public static string ADWSXmlParserInvalidelement;

        // Token: 0x040000B8 RID: 184
        public static string ADWSXmlParserUnexpectedElement;

        // Token: 0x040000B9 RID: 185
        public static string ADWSXmlParserInvalidAttribute;

        // Token: 0x040000BA RID: 186
        public static string ADWSXmlParserMandatoryHeaderNotUnderstood;

        // Token: 0x040000BB RID: 187
        public static string ADWSXmlParserEmptyMessageReceived;

        // Token: 0x040000BC RID: 188
        public static string ADWSXmlParserInvalidActionForMessage;

        // Token: 0x040000BD RID: 189
        public static string ADProviderSetItemNotSupported;

        // Token: 0x040000BE RID: 190
        public static string ADProviderClearItemNotSupported;

        // Token: 0x040000BF RID: 191
        public static string ADProviderExecuteItemNotSupported;

        // Token: 0x040000C0 RID: 192
        public static string ADProviderCopyItemNotSupported;

        // Token: 0x040000C1 RID: 193
        public static string ADProviderPathNotFound;

        // Token: 0x040000C2 RID: 194
        public static string ADProviderPropertiesNotSpecified;

        // Token: 0x040000C3 RID: 195
        public static string ADProviderPropertyValueCannotBeNull;

        // Token: 0x040000C4 RID: 196
        public static string ADProviderPropertiesToClearNotSpecified;

        // Token: 0x040000C5 RID: 197
        public static string ADProviderSDNotSet;

        // Token: 0x040000C6 RID: 198
        public static string ADProviderGCInvalidForADLDS;

        // Token: 0x040000C7 RID: 199
        public static string ADProviderGCInvalidWithAppendedPort;

        // Token: 0x040000C8 RID: 200
        public static string ADProviderUnableToGetPartition;

        // Token: 0x040000C9 RID: 201
        public static string ADProviderOperationNotSupportedForRootDSE;

        // Token: 0x040000CA RID: 202
        public static string ADProviderOperationNotSupportedForRootDSEUnlessGC;

        // Token: 0x040000CB RID: 203
        public static string ADProviderInvalidPropertyName;

        // Token: 0x040000CC RID: 204
        public static string ADProviderUnableToReadProperty;

        // Token: 0x040000CD RID: 205
        public static string ADProviderErrorInitializingDefaultDrive;

        // Token: 0x040000CE RID: 206
        public static string ProviderUtilInvalidDrivePath;

        // Token: 0x040000CF RID: 207
        public static string TranslateNameError;

        // Token: 0x040000D0 RID: 208
        public static string LoadingDriveProgressMessage;

        // Token: 0x040000D1 RID: 209
        public static string UnspecifiedError;

        // Token: 0x040000D2 RID: 210
        public static string OperationSuccessful;

        // Token: 0x040000D3 RID: 211
        public static string NoSchemaClassInSchemaCache;

        // Token: 0x040000D4 RID: 212
        public static string SessionRequired;

        // Token: 0x040000D5 RID: 213
        public static string CustomAttributeCollision;

        // Token: 0x040000D6 RID: 214
        public static string MultipleKeywords;

        // Token: 0x040000D7 RID: 215
        public static string InvalidParameterValue;

        // Token: 0x040000D8 RID: 216
        public static string InvalidHashtableKey;

        // Token: 0x040000D9 RID: 217
        public static string InvalidNullValue;

        // Token: 0x040000DA RID: 218
        public static string InvalidEmptyCollection;

        // Token: 0x040000DB RID: 219
        public static string InvalidNullInCollection;

        // Token: 0x040000DC RID: 220
        public static string InvalidParameterType;

        // Token: 0x040000DD RID: 221
        public static string ParameterRequired;

        // Token: 0x040000DE RID: 222
        public static string ParameterRequiredMultiple;

        // Token: 0x040000DF RID: 223
        public static string ParameterRequiredOnlyOne;

        // Token: 0x040000E0 RID: 224
        public static string ObjectNotFound;

        // Token: 0x040000E1 RID: 225
        public static string InstanceMustBeOfType;

        // Token: 0x040000E2 RID: 226
        public static string InvalidObjectClass;

        // Token: 0x040000E3 RID: 227
        public static string InvalidObjectClasses;

        // Token: 0x040000E4 RID: 228
        public static string MultipleMatches;

        // Token: 0x040000E5 RID: 229
        public static string InvalidHashtableKeyType;

        // Token: 0x040000E6 RID: 230
        public static string InvalidTypeInCollection;

        // Token: 0x040000E7 RID: 231
        public static string ObjectTypeNotEqualToExpectedType;

        // Token: 0x040000E8 RID: 232
        public static string DistinguishedNameCannotBeNull;

        // Token: 0x040000E9 RID: 233
        public static string FSMORoleNotFoundInDomain;

        // Token: 0x040000EA RID: 234
        public static string FSMORoleNotFoundInForest;

        // Token: 0x040000EB RID: 235
        public static string SearchConverterRHSNotDataNode;

        // Token: 0x040000EC RID: 236
        public static string SearchConverterNotBinaryNode;

        // Token: 0x040000ED RID: 237
        public static string SearchConverterSupportedOperatorListErrorMessage;

        // Token: 0x040000EE RID: 238
        public static string SearchConverterInvalidValue;

        // Token: 0x040000EF RID: 239
        public static string SearchConverterUnrecognizedObjectType;

        // Token: 0x040000F0 RID: 240
        public static string SearchConverterIdentityAttributeNotSet;

        // Token: 0x040000F1 RID: 241
        public static string SearchConverterRHSInvalidType;

        // Token: 0x040000F2 RID: 242
        public static string SearchConverterAttributeNotSupported;

        // Token: 0x040000F3 RID: 243
        public static string SearchConverterUseSearchADAccount;

        // Token: 0x040000F4 RID: 244
        public static string SearchConverterRHSNotMatchEnumValue;

        // Token: 0x040000F5 RID: 245
        public static string AttributeConverterUnrecognizedObjectType;

        // Token: 0x040000F6 RID: 246
        public static string InsufficientPermissionsToProtectObject;

        // Token: 0x040000F7 RID: 247
        public static string InsufficientPermissionsToProtectObjectParent;

        // Token: 0x040000F8 RID: 248
        public static string CannotResolveIPAddressToHostName;

        // Token: 0x040000F9 RID: 249
        public static string WarningSamAccountNameClauseLacksDollarSign;

        // Token: 0x040000FA RID: 250
        public static string FailedAddingMembersToGroup;

        // Token: 0x040000FB RID: 251
        public static string FailedAddingMembersToOneOrMoreGroup;

        // Token: 0x040000FC RID: 252
        public static string FailedRemovingMembersFromGroup;

        // Token: 0x040000FD RID: 253
        public static string FailedRemovingMembersFromOneOrMoreGroup;

        // Token: 0x040000FE RID: 254
        public static string PasswordRestrictionErrorMessage;

        // Token: 0x040000FF RID: 255
        public static string ChangePasswordErrorMessage;

        // Token: 0x04000100 RID: 256
        public static string UserPressedBreakDuringPasswordEntry;

        // Token: 0x04000101 RID: 257
        public static string PromptForCurrentPassword;

        // Token: 0x04000102 RID: 258
        public static string PromptForNewPassword;

        // Token: 0x04000103 RID: 259
        public static string PromptForRepeatPassword;

        // Token: 0x04000104 RID: 260
        public static string PasswordsDidNotMatch;

        // Token: 0x04000105 RID: 261
        public static string PasswordChangeSuccessful;

        // Token: 0x04000106 RID: 262
        public static string MethodNotSupportedForObjectType;

        // Token: 0x04000107 RID: 263
        public static string UnsupportedObjectClass;

        // Token: 0x04000108 RID: 264
        public static string AttributeNotFoundOnObject;

        // Token: 0x04000109 RID: 265
        public static string WarningPolicyUsageNotAccurateOnRODC;

        // Token: 0x0400010A RID: 266
        public static string WarningResultantPRPNotAccurateOnRODC;

        // Token: 0x0400010B RID: 267
        public static string ErrorResultantPRPSpecifyWindows2008OrAbove;

        // Token: 0x0400010C RID: 268
        public static string UnsupportedOptionSpecified;

        // Token: 0x0400010D RID: 269
        public static string MoveOperationMasterRoleCaption;

        // Token: 0x0400010E RID: 270
        public static string MoveOperationMasterRoleWarning;

        // Token: 0x0400010F RID: 271
        public static string MoveOperationMasterRoleDescription;

        // Token: 0x04000110 RID: 272
        public static string MoveOperationMasterRoleNotApplicableForADLDS;

        // Token: 0x04000111 RID: 273
        public static string DuplicateValuesSpecified;

        // Token: 0x04000112 RID: 274
        public static string CouldNotDetermineLoggedOnUserDomain;

        // Token: 0x04000113 RID: 275
        public static string CouldNotDetermineLocalComputerDomain;

        // Token: 0x04000114 RID: 276
        public static string RequiresDomainCredentials;

        // Token: 0x04000115 RID: 277
        public static string UseSetADDomainMode;

        // Token: 0x04000116 RID: 278
        public static string UseSetADForestMode;

        // Token: 0x04000117 RID: 279
        public static string ADInvalidQuantizationDays;

        // Token: 0x04000118 RID: 280
        public static string ADInvalidQuantizationHours;

        // Token: 0x04000119 RID: 281
        public static string ADInvalidQuantizationMinutes;

        // Token: 0x0400011A RID: 282
        public static string ADInvalidQuantizationFifteenMinuteIntervals;

        // Token: 0x0400011B RID: 283
        public static string CannotInstallServiceAccount;

        // Token: 0x0400011C RID: 284
        public static string ServiceAccountIsNotInstalled;

        // Token: 0x0400011D RID: 285
        public static string CannotResetPasswordOfServiceAccount;

        // Token: 0x0400011E RID: 286
        public static string CannotUninstallServiceAccount;

        // Token: 0x0400011F RID: 287
        public static string CannotTestServiceAccount;

        // Token: 0x04000120 RID: 288
        public static string NetAddServiceAccountFailed;

        // Token: 0x04000121 RID: 289
        public static string CannotReachHostingDC;

        // Token: 0x04000122 RID: 290
        public static string OtherBackLinkDescription;

        // Token: 0x04000123 RID: 291
        public static string OtherBackLinkCaption;

        // Token: 0x04000124 RID: 292
        public static string ServiceAccountNameLengthInvalid;

        // Token: 0x04000125 RID: 293
        public static string AcctChangePwdNotWorksWhenPwdNotExpires;

        // Token: 0x04000126 RID: 294
        public static string AddADPrincipalGroupMembershipShouldProcessCaption;

        // Token: 0x04000127 RID: 295
        public static string AddADPrincipalGroupMembershipShouldProcessWarning;

        // Token: 0x04000128 RID: 296
        public static string AddADPrincipalGroupMembershipShouldProcessDescription;

        // Token: 0x04000129 RID: 297
        public static string RemoveADPrincipalGroupMembershipShouldProcessCaption;

        // Token: 0x0400012A RID: 298
        public static string RemoveADPrincipalGroupMembershipShouldProcessWarning;

        // Token: 0x0400012B RID: 299
        public static string RemoveADPrincipalGroupMembershipShouldProcessDescription;

        // Token: 0x0400012C RID: 300
        public static string ParameterValueNotSearchResult;

        // Token: 0x0400012D RID: 301
        public static string GetGroupMembershipResourceContextParameterCheck;

        // Token: 0x0400012E RID: 302
        public static string IdentityResolutionPartitionRequired;

        // Token: 0x0400012F RID: 303
        public static string IdentityInExtendedAttributeCannotBeResolved;

        // Token: 0x04000130 RID: 304
        public static string IdentityNotFound;

        // Token: 0x04000131 RID: 305
        public static string IdentityInWrongPartition;

        // Token: 0x04000132 RID: 306
        public static string DirectoryServerNotFound;

        // Token: 0x04000133 RID: 307
        public static string EnablingIsIrreversible;

        // Token: 0x04000134 RID: 308
        public static string CouldNotSetForestMode;

        // Token: 0x04000135 RID: 309
        public static string CouldNotFindForestIdentity;

        // Token: 0x04000136 RID: 310
        public static string ConnectedToWrongForest;

        // Token: 0x04000137 RID: 311
        public static string EmptySearchBaseNotSupported;

        // Token: 0x04000138 RID: 312
        public static string PromptForRemove;

        // Token: 0x04000139 RID: 313
        public static string PromptForRecursiveRemove;

        // Token: 0x0400013A RID: 314
        public static string PerformingRecursiveRemove;

        // Token: 0x0400013B RID: 315
        public static string ConfigSetNotFound;

        // Token: 0x0400013C RID: 316
        public static string ADInvalidAttributeValueCount;

        // Token: 0x0400013D RID: 317
        public static string ServerContainerNotEmpty;

        // Token: 0x0400013E RID: 318
        public static string InvalidClaimValueType;

        // Token: 0x0400013F RID: 319
        public static string InvalidPossibleValuesXml;

        // Token: 0x04000140 RID: 320
        public static string NextVersionPossibleValuesXml;

        // Token: 0x04000141 RID: 321
        public static string CannotOverwriteNextVersionXml;

        // Token: 0x04000142 RID: 322
        public static string SPCTNoSourceWarning;

        // Token: 0x04000143 RID: 323
        public static string SPCTBothSourceWarning;

        // Token: 0x04000144 RID: 324
        public static string SPCTBothSourceOIDPossibleValuesWarning;

        // Token: 0x04000145 RID: 325
        public static string CTBothPossibleValuesShareValueWarning;

        // Token: 0x04000146 RID: 326
        public static string SPCTInvalidAppliesToClassWarning;

        // Token: 0x04000147 RID: 327
        public static string CTParameterValidationFailure;

        // Token: 0x04000148 RID: 328
        public static string SPCTInvalidSourceAttributeName;

        // Token: 0x04000149 RID: 329
        public static string SPCTBlockedSourceAttribute;

        // Token: 0x0400014A RID: 330
        public static string SPCTNonREPLSourceAttrError;

        // Token: 0x0400014B RID: 331
        public static string SPCTRODCFilteredSourceAttr;

        // Token: 0x0400014C RID: 332
        public static string SPCTDefuctSourceAttr;

        // Token: 0x0400014D RID: 333
        public static string SPCTInvalidAttributeSyntax;

        // Token: 0x0400014E RID: 334
        public static string CTSourceOIDValueTypeError;

        // Token: 0x0400014F RID: 335
        public static string CTSourceAttributeValueTypeError;

        // Token: 0x04000150 RID: 336
        public static string RCTNoResourcePropertyValueTypeError;

        // Token: 0x04000151 RID: 337
        public static string InvalidValueTypeForPossibleValueXml;

        // Token: 0x04000152 RID: 338
        public static string SPCTSourceAttributeLdapDisplayNameError;

        // Token: 0x04000153 RID: 339
        public static string SPCTAttributeNotFoundInSchemaClass;

        // Token: 0x04000154 RID: 340
        public static string CAPIDCreationFailure;

        // Token: 0x04000155 RID: 341
        public static string CAPMemberMaximumExceeded;

        // Token: 0x04000156 RID: 342
        public static string SPCTInvalidSourceAttribute;

        // Token: 0x04000157 RID: 343
        public static string RCTSuggestedValueNotPresentError;

        // Token: 0x04000158 RID: 344
        public static string RCTSuggestedValuePresentError;

        // Token: 0x04000159 RID: 345
        public static string ResourcePropertySharesValueWithValueTypeError;

        // Token: 0x0400015A RID: 346
        public static string SuggestedValueNotUniqueError;

        // Token: 0x0400015B RID: 347
        public static string ADTrustNoDirectionAndPolicyError;

        // Token: 0x0400015C RID: 348
        public static string ClaimPolicyXmlWarning;

        // Token: 0x0400015D RID: 349
        public static string ClaimPolicyXmlNodeError;

        // Token: 0x0400015E RID: 350
        public static string ServerParameterNotSupported;

        // Token: 0x0400015F RID: 351
        public static string XmlFormattingError;

        // Token: 0x04000160 RID: 352
        public static string RuleValidationFailure;

        // Token: 0x04000161 RID: 353
        public static string ResouceConditionValidationFailed;

        // Token: 0x04000162 RID: 354
        public static string SDDLValidationFailed;

        // Token: 0x04000163 RID: 355
        public static string DisplayNameNotUniqueError;

        // Token: 0x04000164 RID: 356
        public static string RemoveClaimTypeSharesValueWithError;

        // Token: 0x04000165 RID: 357
        public static string SharesValueWithIdentityError;

        // Token: 0x04000166 RID: 358
        public static string ServerTargetParameterNotSpecified;

        // Token: 0x04000167 RID: 359
        public static string TargetParameterHM;

        // Token: 0x04000168 RID: 360
        public static string ClaimIDValidationError;

        // Token: 0x04000169 RID: 361
        public static string ResourceIDValidationError;

        // Token: 0x0400016A RID: 362
        public static string ClaimTypeRestrictValueError;

        // Token: 0x0400016B RID: 363
        public static string RemoveCmdletBacklinkWarning;

        // Token: 0x0400016C RID: 364
        public static string TicksToMinsRoundOffWarning;

        // Token: 0x0400016D RID: 365
        public static string DefaultAuthenticationPolicyExpressionTitle;

        // Token: 0x0400016E RID: 366
        public static string DefaultAuthenticationPolicyExpressionMessage;

        // Token: 0x0400016F RID: 367
        public static string InitializeClaimDictionaryError;

        // Token: 0x04000170 RID: 368
        public static string EditConditionalAceClaimsError;

        // Token: 0x04000171 RID: 369
        public static string DomainModeDeprecatedWarning;

        // Token: 0x04000172 RID: 370
        public static string ForestModeDeprecatedWarning;

        // Token: 0x04000173 RID: 371
        public static string PropertyIsReadonly;

        // Token: 0x04000174 RID: 372
        public static string NoConversionExists;

        // Token: 0x04000175 RID: 373
        public static string TypeConversionError;

        // Token: 0x04000176 RID: 374
        public static string TypeAdapterForADEntityOnly;

        // Token: 0x04000177 RID: 375
        public static string EnumConversionError;

        // Token: 0x04000178 RID: 376
        public static string ServerActionNotSupportedFault;

        // Token: 0x04000179 RID: 377
        public static string ServerCannotProcessFilter;

        // Token: 0x0400017A RID: 378
        public static string ServerEncodingLimit;

        // Token: 0x0400017B RID: 379
        public static string ServerEnumerationContextLimitExceeded;

        // Token: 0x0400017C RID: 380
        public static string ServerFilterDialectRequestedUnavailable;

        // Token: 0x0400017D RID: 381
        public static string ServerFragmentDialectNotSupported;

        // Token: 0x0400017E RID: 382
        public static string ServerInvalidEnumerationContext;

        // Token: 0x0400017F RID: 383
        public static string ServerInvalidExpirationTime;

        // Token: 0x04000180 RID: 384
        public static string ServerSchemaValidationError;

        // Token: 0x04000181 RID: 385
        public static string ServerUnsupportedSelectOrSortDialectFault;

        // Token: 0x04000182 RID: 386
        public static string ServerAnonymousNotAllowed;

        // Token: 0x04000183 RID: 387
        public static string ServerInvalidInstance;

        // Token: 0x04000184 RID: 388
        public static string ServerMultipleMatchingSecurityPrincipals;

        // Token: 0x04000185 RID: 389
        public static string ServerNoMatchingSecurityPrincipal;

        // Token: 0x04000186 RID: 390
        public static string InvalidProperty;

        // Token: 0x04000187 RID: 391
        public static string InvalidFilter;

        // Token: 0x04000188 RID: 392
        public static string AsqResponseError;

        // Token: 0x04000189 RID: 393
        public static string ADAccountRPRPIdentityHM;

        // Token: 0x0400018A RID: 394
        public static string ADAccountRPRPDomainControllerHM;

        // Token: 0x0400018B RID: 395
        public static string ADDCPRPUIdentityHM;

        // Token: 0x0400018C RID: 396
        public static string ADObjectFilterHM;

        // Token: 0x0400018D RID: 397
        public static string ADOUFilterHM;

        // Token: 0x0400018E RID: 398
        public static string ADComputerServiceAccountIdentityHM;

        // Token: 0x0400018F RID: 399
        public static string ADFineGrainedPPFilterHM;

        // Token: 0x04000190 RID: 400
        public static string ADFGPPSubjectIdentityHM;

        // Token: 0x04000191 RID: 401
        public static string ADGroupFilterHM;

        // Token: 0x04000192 RID: 402
        public static string ADGroupMemberIdentityHM;

        // Token: 0x04000193 RID: 403
        public static string ADDCPRPIdentityHM;

        // Token: 0x04000194 RID: 404
        public static string ADPrincipalGMIdentityHM;

        // Token: 0x04000195 RID: 405
        public static string ADOFFilterHM;

        // Token: 0x04000196 RID: 406
        public static string ADUserFilterHM;

        // Token: 0x04000197 RID: 407
        public static string ADAccountAuthGroupIdentityHM;

        // Token: 0x04000198 RID: 408
        public static string ADUserResultantPPIdentityHM;

        // Token: 0x04000199 RID: 409
        public static string ADServiceAccountFilterHM;

        // Token: 0x0400019A RID: 410
        public static string ADComputerFilterHM;

        // Token: 0x0400019B RID: 411
        public static string NullOrEmptyIdentityPropertyArgument;

        // Token: 0x0400019C RID: 412
        public static string DelegatePipelineEmptyError;

        // Token: 0x0400019D RID: 413
        public static string DelegatePipelineUnsupportedTypeError;

        // Token: 0x0400019E RID: 414
        public static string DelegatePipelineMulticastDelegatesNotAllowedError;

        // Token: 0x0400019F RID: 415
        public static string DelegatePipelineReferenceDelegateNotFoundError;

        // Token: 0x040001A0 RID: 416
        public static string ValidateRangeLessThanMinValue;

        // Token: 0x040001A1 RID: 417
        public static string ValidateRangeGreaterThanMaxValue;

        // Token: 0x040001A2 RID: 418
        public static string ObjectToReplicateNotFoundOnSource;

        // Token: 0x040001A3 RID: 419
        public static string SourceServerDown;

        // Token: 0x040001A4 RID: 420
        public static string PasswordOnlySwitchAllowedOnlyOnRODC;

        // Token: 0x040001A5 RID: 421
        public static string DestinationDoesNotTargetDirectoryServer;

        // Token: 0x040001A6 RID: 422
        public static string SourceDoesNotTargetDirectoryServer;

        // Token: 0x040001A7 RID: 423
        public static string DestinationServerDown;

        // Token: 0x040001A8 RID: 424
        public static string SourceServerObjNotFoundOrObjToReplicateNotFound;

        // Token: 0x040001A9 RID: 425
        public static string DestinationServerDoesNotSupportSynchronizingObject;

        // Token: 0x040001AA RID: 426
        public static string SiteLinkAndSiteLinkBridgeDoNotShareSameTransportType;

        // Token: 0x040001AB RID: 427
        public static string NoMatchingResultsForTarget;

        // Token: 0x040001AC RID: 428
        public static string OnlySearchResultsSupported;

        // Token: 0x040001AD RID: 429
        public static string UnsupportedParameterType;

        // Token: 0x040001AE RID: 430
        public static string ServerDoesNotHaveFriendlyPartition;

        // Token: 0x040001AF RID: 431
        public static string ServerIsNotDirectoryServer;

        // Token: 0x040001B0 RID: 432
        public static string UnableToFindSiteForLocalMachine;

        // Token: 0x040001B1 RID: 433
        public static string MsaStandloneNotLinked;

        // Token: 0x040001B2 RID: 434
        public static string MsaStandaloneLinkedToAlternateComputer;

        // Token: 0x040001B3 RID: 435
        public static string MsaDoesNotExist;

        // Token: 0x040001B4 RID: 436
        public static string MsaNotServiceAccount;

        // Token: 0x040001B5 RID: 437
        public static string InvalidACEInSecDesc;

        // Token: 0x040001B6 RID: 438
        public static string ADDCCloningExcludedApplicationListErrorMessage;

        // Token: 0x040001B7 RID: 439
        public static string ADDCCloningExcludedApplicationListCustomerAllowListFileNameMessage;

        // Token: 0x040001B8 RID: 440
        public static string ADDCCloningExcludedApplicationListNoCustomerAllowListFileMessage;

        // Token: 0x040001B9 RID: 441
        public static string ADDCCloningExcludedApplicationListInvalidPath;

        // Token: 0x040001BA RID: 442
        public static string ADDCCloningExcludedApplicationListNoAppsFound;

        // Token: 0x040001BB RID: 443
        public static string ADDCCloningExcludedApplicationListFilePriority;

        // Token: 0x040001BC RID: 444
        public static string ADDCCloningExcludedApplicationListPathPriority;

        // Token: 0x040001BD RID: 445
        public static string ADDCCloningExcludedApplicationListFileExists;

        // Token: 0x040001BE RID: 446
        public static string ADDCCloningExcludedApplicationListNewAllowList;

        // Token: 0x040001BF RID: 447
        public static string ADDCCloningExcludedApplicationListLocalMachineNotADCMessage;

        // Token: 0x040001C0 RID: 448
        public static string NewADDCCloneConfigFileLocatingWin8PDCMessage;

        // Token: 0x040001C1 RID: 449
        public static string NewADDCCloneConfigFileNoWin8PDCMessage;

        // Token: 0x040001C2 RID: 450
        public static string NewADDCCloneConfigFileFoundWin8PDCMessage;

        // Token: 0x040001C3 RID: 451
        public static string NewADDCCloneConfigFileCheckCloningPrivilegeMessage;

        // Token: 0x040001C4 RID: 452
        public static string NewADDCCloneConfigFileNoLocalDCMessage;

        // Token: 0x040001C5 RID: 453
        public static string NewADDCCloneConfigFileFoundLocalDCMessage;

        // Token: 0x040001C6 RID: 454
        public static string NewADDCCloneConfigFileNoLocalDCMembershipMessage;

        // Token: 0x040001C7 RID: 455
        public static string NewADDCCloneConfigFileNoCloningPrivilegeMessage;

        // Token: 0x040001C8 RID: 456
        public static string NewADDCCloneConfigFileFailQueryRootObject;

        // Token: 0x040001C9 RID: 457
        public static string NewADDCCloneConfigFileHasCloningPrivilegeMessage;

        // Token: 0x040001CA RID: 458
        public static string NewADDCCloneConfigFileTestWhiteListMessage;

        // Token: 0x040001CB RID: 459
        public static string NewADDCCloneConfigFileWhiteListCompleteMessage;

        // Token: 0x040001CC RID: 460
        public static string NewADDCCloneConfigFileWhiteListNotCompleteMessage;

        // Token: 0x040001CD RID: 461
        public static string NewADDCCloneConfigFileOfflineModeMessage;

        // Token: 0x040001CE RID: 462
        public static string NewADDCCloneConfigFileLocalModeMessage;

        // Token: 0x040001CF RID: 463
        public static string NewADDCCloneConfigFileGenerateFileMessage;

        // Token: 0x040001D0 RID: 464
        public static string NewADDCCloneConfigFileNoGenerateFileMessage;

        // Token: 0x040001D1 RID: 465
        public static string NewADDCCloneConfigFileGetDitLocationMessage;

        // Token: 0x040001D2 RID: 466
        public static string NewADDCCloneConfigFileNoDitLocationMessage;

        // Token: 0x040001D3 RID: 467
        public static string NewADDCCloneConfigFileGenerationMessage;

        // Token: 0x040001D4 RID: 468
        public static string NewADDCCloneConfigFileFullNameMessage;

        // Token: 0x040001D5 RID: 469
        public static string NewADDCCloneConfigFileGeneratingContentMessage;

        // Token: 0x040001D6 RID: 470
        public static string NewADDCCloneConfigFileGeneratedMessage;

        // Token: 0x040001D7 RID: 471
        public static string NewADDCCloneConfigFileExistingMessage;

        // Token: 0x040001D8 RID: 472
        public static string NewADDCCloneConfigFileNotExistingMessage;

        // Token: 0x040001D9 RID: 473
        public static string NewADDCCloneConfigFileFoundMessage;

        // Token: 0x040001DA RID: 474
        public static string NewADDCCloneConfigFileAtWrongLocationMessage;

        // Token: 0x040001DB RID: 475
        public static string NewADDCCloneConfigFileInvalidIpv4StaticMessage;

        // Token: 0x040001DC RID: 476
        public static string NewADDCCloneConfigFileMoreDnsMessage;

        // Token: 0x040001DD RID: 477
        public static string NewADDCCloneConfigFileLocalModeNoLocalDCMessage;
    }
}

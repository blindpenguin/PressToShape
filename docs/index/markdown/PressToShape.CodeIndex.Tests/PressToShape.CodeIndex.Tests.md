# PressToShape.CodeIndex.Tests

Project: PressToShape.CodeIndex.Tests

## ApplicationFactsTests (class)

`public class ApplicationFactsTests`

Source: `tests/PressToShape.CodeIndex.Tests/Avalonia/ApplicationFactsTests.cs:10-177`

_(no documentation)_

**Members**

- `public void Extract_Converters_ListBothInterfacesAndIgnoreLookAlikes()` - _(no documentation)_
- `public void Extract_Application_HasTheClassAndItsLifecycleOverridesOnly()` - _(no documentation)_
- `public void Extract_Initialize_ListsCreatedSolutionTypesAndIgnoresFrameworkTypes()` - _(no documentation)_
- `public void Extract_LifetimeBranches_HaveTheirOwnCreations()` - _(no documentation)_
- `public void Extract_CreationOutsideAnyBranch_IsInTheMethodListAndNotInTheBranches()` - _(no documentation)_
- `public void Extract_BranchAndMethodLines_AreTheLinesOfTheCode()` - _(no documentation)_
- `public void Extract_NonOverrideMethodsAndOtherClasses_AreIgnored()` - _(no documentation)_
- `public void Extract_NoApplicationOrConverters_GivesEmptyLists()` - _(no documentation)_
- `public void Extract_OverrideWithoutCreations_IsStillListed()` - _(no documentation)_
- `public void Extract_TwoRuns_GiveTheSameFacts()` - _(no documentation)_

## AtomicFileWriterTests (class)

`public sealed class AtomicFileWriterTests : IDisposable`

Source: `tests/PressToShape.CodeIndex.Tests/AtomicFileWriterTests.cs:11-161`

The only tests that touch the file system, each in its own unique temporary folder that is deleted afterwards.

**Members**

- `public void Dispose()` - _(no documentation)_
- `public void Write_NewFile_CreatesFoldersAndContent()` - _(no documentation)_
- `public void Write_ExistingFile_IsReplacedAndNoTemporaryFileRemains()` - _(no documentation)_
- `public void Write_TargetCannotBeReplaced_LeavesNoTemporaryFile()` - _(no documentation)_
- `public void Write_PathOutsideRoot_IsRejectedAndNothingIsWritten(string path)` - _(no documentation)_
- `public void RemoveStale_DeletesFilesNotWrittenAndEmptyFolders()` - _(no documentation)_
- `public void RemoveStale_NeverTouchesFilesOutsideTheRoot()` - _(no documentation)_
- `public void RemoveStale_RootDoesNotExist_ReturnsNothing()` - _(no documentation)_
- `public void InMemoryFileSink_Write_KeepsFilesSortedAndRejectsBadPaths()` - _(no documentation)_
- `public void Write_FileWithTheOldFixedTemporaryNameExists_IsNeitherChangedNorRemoved()` - _(no documentation)_
- `public void RemoveStale_LeftoverUniqueTemporaryFile_IsRemovedAndReported()` - _(no documentation)_
- `public void Write_TwoWritesToDifferentFiles_UseDifferentTemporaryNames()` - _(no documentation)_

## AvaloniaModelTests (class)

`public class AvaloniaModelTests`

Source: `tests/PressToShape.CodeIndex.Tests/Avalonia/AvaloniaModelTests.cs:10-88`

_(no documentation)_

**Members**

- `public void RelationshipType_ExecutesMethod_IsAppendedLast()` - _(no documentation)_
- `public void IndexSchema_Version_IsThree()` - _(no documentation)_
- `public void IndexData_WithoutFacts_HasNullAvalonia()` - _(no documentation)_
- `public void IndexData_WithFacts_KeepsThem()` - _(no documentation)_
- `public void AvaloniaFacts_Empty_HasNoItems()` - _(no documentation)_
- `public void Facts_AreWritableAsPlainDataAndReadBack()` - _(no documentation)_
- `public void Writer_DoesNotWriteFactsAsFiles()` - _(no documentation)_

## AvaloniaPagesPartTwoTests (class)

`public class AvaloniaPagesPartTwoTests`

Source: `tests/PressToShape.CodeIndex.Tests/Avalonia/AvaloniaPagesPartTwoTests.cs:13-172`

_(no documentation)_

**Members**

- `public void Styles_ListFilesWithCountsAndVariants()` - _(no documentation)_
- `public void Styles_ListKeysThatAreMissingOrUnused()` - _(no documentation)_
- `public void Styles_GraphStartsAtTheApplicationFile()` - _(no documentation)_
- `public void Services_ListInterfacesWithImplementationsUsersAndCreationSites()` - _(no documentation)_
- `public void Services_WithoutRegistrations_NameTheMembersThatCreateServices()` - _(no documentation)_
- `public Task Services_WithARegistrationAndAnInterfaceWithoutImplementation_ListThem()` - _(no documentation)_
- `public void Application_ListsTheClassItsLifetimeBranchesAndTheConvention()` - _(no documentation)_
- `public void Application_ConventionListsOnlyViewLocatorReasons()` - _(no documentation)_
- `public void Application_ListsContentHostsAsInferredAndClaimsNoNavigation()` - _(no documentation)_
- `public void Application_WithoutAnApplicationClass_SaysNoneFound()` - _(no documentation)_
- `public void Pages_AreDeterministicAndFreeOfAbsolutePathsAndTheWordKind(string name)` - _(no documentation)_
- `public void Readme_LinksAllSixPages()` - _(no documentation)_

## AvaloniaPagesTests (class)

`public class AvaloniaPagesTests`

Source: `tests/PressToShape.CodeIndex.Tests/Avalonia/AvaloniaPagesTests.cs:13-128`

_(no documentation)_

**Members**

- `public void Views_ListsViewsWithViewModelsEvidenceAndCounts()` - _(no documentation)_
- `public void Views_MarksInferredLinksWithTheirReason()` - _(no documentation)_
- `public Task Views_DoNotListTheApplicationClass()` - _(no documentation)_
- `public void Views_ListsViewModelsWithoutAViewAndSaysNoneForViewsWithoutAViewModel()` - _(no documentation)_
- `public void Controls_ListsPropertiesAndSaysNoneFoundForEvents()` - _(no documentation)_
- `public void Converters_ShowUsesAndMarkAnUnusedConverter()` - _(no documentation)_
- `public void Readme_LinksTheAvaloniaPages()` - _(no documentation)_
- `public void Pages_AreDeterministicAndFreeOfAbsolutePathsAndTheWordKind(string name)` - _(no documentation)_
- `public void Pages_WithoutFacts_SayNoneFound()` - _(no documentation)_

## AvaloniaPagesWriterAccess (class)

`internal static class AvaloniaPagesWriterAccess`

Source: `tests/PressToShape.CodeIndex.Tests/Avalonia/AvaloniaPagesTests.cs:130-134`

_(no documentation)_

**Members**

- `public void Write(IndexData data, IIndexFileSink sink)` - _(no documentation)_

## BindingResolverTests (class)

`public class BindingResolverTests`

Source: `tests/PressToShape.CodeIndex.Tests/Xaml/BindingResolverTests.cs:10-281`

_(no documentation)_

**Members**

- `public void Resolve_SimpleProperty_IsVerifiedWithThePropertyId()` - _(no documentation)_
- `public void Resolve_NestedPath_FollowsMemberTypesAndReportsTheSolutionTypeId()` - _(no documentation)_
- `public void Resolve_NullableReferenceAndNullableValueTypes_ContinueWithTheUnderlyingType()` - _(no documentation)_
- `public void Resolve_InheritedMember_IsFoundOnTheBaseType()` - _(no documentation)_
- `public void Resolve_InterfaceMember_IsFoundThroughBaseInterfaces()` - _(no documentation)_
- `public void Resolve_Field_IsAllowed()` - _(no documentation)_
- `public void Resolve_CollectionProperty_ContinuesWithTheCollectionMembers()` - _(no documentation)_
- `public void Resolve_CommandProperty_IsMarkedAsCommand()` - _(no documentation)_
- `public void Resolve_GeneratedCommandProperty_IsFound()` - _(no documentation)_
- `public void Resolve_Negation_IsIgnoredForTheTarget(string path)` - _(no documentation)_
- `public void Resolve_DataContextItself_IsVerifiedWithoutAProperty(string? path)` - _(no documentation)_
- `public void Resolve_UnmodelledForms_AreUnresolvedWithAReasonAndNeverVerified(string path, string expectedPart)` - _(no documentation)_
- `public void Resolve_UnmodelledFormAfterResolvedSegments_SaysHowFarItGot()` - _(no documentation)_
- `public void Resolve_MissingSegment_NamesTheSegmentAndTheType()` - _(no documentation)_
- `public void Resolve_PrivateMemberAndIndexerAndMethod_AreNotBindable()` - _(no documentation)_
- `public void Resolve_NoDataType_IsUnresolvedWithTheReason()` - _(no documentation)_
- `public void Resolve_OtherSources_AreUnresolvedWithoutAKnownType(string path)` - _(no documentation)_
- `public void ResolveSegments_FromAKnownStartType_IsVerified()` - _(no documentation)_
- `public void ResolveSegments_NoSegmentsAndNoUnsupported_IsTheStartTypeItself()` - _(no documentation)_
- `public void Parse_Sources_AreRecognized(string path, string source, string? sourceText, string segments)` - _(no documentation)_
- `public void Parse_LeadingBangs_AreCounted()` - _(no documentation)_
- `public void Parse_BadInput_NeverThrowsAndReportsUnsupported(string path)` - _(no documentation)_

## ChainViewModelTests (class)

`public class ChainViewModelTests`

Source: `tests/PressToShape.CodeIndex.Tests/Chain/ChainViewModelTests.cs:15-157`

_(no documentation)_

**Members**

- `public void Build_ViewModel_ListsTheViewsThatUseItWithEvidence()` - _(no documentation)_
- `public void Build_ViewModelWithAVerifiedAndAnInferredLink_ListsBoth()` - _(no documentation)_
- `public void Build_ViewModel_ListsCommandsWithHandlersAndTheXamlPlacesThatBindThem()` - _(no documentation)_
- `public void Build_ViewModel_HasServicesModelsAndTests()` - _(no documentation)_
- `public void Build_GeneratedCommand_ShowsHandlerBindingsAndNoTests()` - _(no documentation)_
- `public void Build_MethodGroupCommandAndParameterCommand_ShowTheirHandlers()` - _(no documentation)_
- `public Task Build_CommandBoundByAButtonAndAKeyBinding_ListsBothPlaces()` - _(no documentation)_
- `public void Build_SymbolsThatAreNotViewsViewModelsOrCommands_AreNotSupported()` - _(no documentation)_
- `public void Build_NotSupportedMessage_NamesTheSymbolType()` - _(no documentation)_

## ChainViewTests (class)

`public class ChainViewTests`

Source: `tests/PressToShape.CodeIndex.Tests/Chain/ChainViewTests.cs:18-250`

_(no documentation)_

**Members**

- `internal ChainBuilder Sample()` - _(no documentation)_
- `internal IEnumerable<ChainNode> All(ChainNode node)` - _(no documentation)_
- `internal ChainNode Find(ChainNode root, string labelStart)` - _(no documentation)_
- `public void Build_View_HasTheViewItsXamlFileAndItsViewModelWithEvidence()` - _(no documentation)_
- `public void Build_View_ListsBindingsWithHandlersOfCommands()` - _(no documentation)_
- `public void Build_View_MethodGroupHandlerAndUnknownHandler()` - _(no documentation)_
- `public void Build_View_ListsServicesWithImplementationsAndCreationSites()` - _(no documentation)_
- `public void Build_View_ListsModelsOfProjectsWithoutViews()` - _(no documentation)_
- `public void Build_View_ListsChildViewsWithVerifiedAndInferredLinks()` - _(no documentation)_
- `public void Build_View_ListsResourcesConvertersAndUnresolvedThemeKeys()` - _(no documentation)_
- `public void Build_View_ListsTestsOfTheViewModelAndSaysNoneForTheView()` - _(no documentation)_
- `public void Build_ViewWithTwoLinksToTheSameViewModel_UsesTheVerifiedOneAndNotesTheOther()` - _(no documentation)_
- `public void Build_ByXamlFileId_GivesTheSameChain()` - _(no documentation)_
- `public void Build_XamlFileWithoutAClass_IsNotSupported()` - _(no documentation)_
- `public void Build_UnknownAndAmbiguousNames_BehaveLikeShow()` - _(no documentation)_
- `public void Build_DepthOne_DoesNotExpandChildViewsAndDepthTwoDoes()` - _(no documentation)_
- `public void Build_DepthIsLimited()` - _(no documentation)_
- `public Task Build_ViewsThatHostEachOther_StopAtTheSecondVisit()` - _(no documentation)_
- `public void Build_Twice_GivesTheSameTreeAndTheViewHasNoChildViewsWhenItHostsNone()` - _(no documentation)_

## CliAppTests (class)

`public class CliAppTests`

Source: `tests/PressToShape.CodeIndex.Tests/CliAppTests.cs:18-526`

_(no documentation)_

**Members**

- `public Task Help_ListsEveryCommandAndOptionAndExitsWithSuccess()` - _(no documentation)_
- `public Task UsageErrors_ExitWithTwoAndPrintUsageToStderr(params string[] args)` - _(no documentation)_
- `public Task Generate_WritesTheIndexReportsCountsAndRemovesStaleFiles()` - _(no documentation)_
- `public Task Generate_ProjectError_WritesNothingKeepsOldFilesAndFails()` - _(no documentation)_
- `public Task Generate_AndroidWarning_StillSucceedsAndIsPrinted()` - _(no documentation)_
- `public Task Generate_SolutionOption_IsResolvedAgainstTheStartFolder()` - _(no documentation)_
- `public Task Generate_Default_UsesTheSolutionAtTheRepoRoot()` - _(no documentation)_
- `public Task Check_FreshIndex_ReportsUpToDateAndWritesNothing()` - _(no documentation)_
- `public Task Check_ChangedMissingAndStaleFiles_AreListed()` - _(no documentation)_
- `public Task Check_ProjectError_FailsWithoutComparing()` - _(no documentation)_
- `public Task Find_PrintsOneLinePerHit()` - _(no documentation)_
- `public Task Find_NoMatch_SaysSoAndSucceeds()` - _(no documentation)_
- `public Task Show_PrintsSignatureDocsAndRelationships()` - _(no documentation)_
- `public Task Show_AmbiguousName_ListsIdsAndFails()` - _(no documentation)_
- `public Task Show_Unknown_SuggestsFindAndFails()` - _(no documentation)_
- `public Task QueryCommands_PrintTheirHits(string command, string argument, string expected)` - _(no documentation)_
- `public Task Json_Find_ProducesValidJsonArray()` - _(no documentation)_
- `public Task Json_ShowRefsAndAmbiguous_AreValidJson()` - _(no documentation)_
- `public Task Query_WithoutIndex_SaysHowToCreateItAndFails()` - _(no documentation)_
- `public Task Query_DoesNotCompileTheSolution()` - _(no documentation)_
- `public Task Generate_TwoIndexFilesCollide_FailsAndLeavesTheExistingIndexAlone()` - _(no documentation)_
- `public Task Check_TwoIndexFilesCollide_FailsWithTheSameMessage()` - _(no documentation)_
- `public Task Generate_NoCollision_WritesTheSameFilesAsBefore()` - _(no documentation)_
- `public Task Query_FolderCannotBeRead_PrintsAFriendlyMessage(string command, string argument)` - _(no documentation)_
- `public Task Check_FolderCannotBeRead_PrintsAFriendlyMessage()` - _(no documentation)_
- `public Task Generate_FolderCannotBeWritten_PrintsAFriendlyMessage()` - _(no documentation)_
- `public Task Query_SecurityProblem_PrintsAFriendlyMessage()` - _(no documentation)_
- `public Task Query_UnexpectedException_StillEscapes()` - _(no documentation)_
- `public Task Query_Cancelled_StillEscapes()` - _(no documentation)_

## CliChainTests (class)

`public class CliChainTests`

Source: `tests/PressToShape.CodeIndex.Tests/Chain/CliChainTests.cs:13-223`

_(no documentation)_

**Members**

- `public Task Chain_View_PrintsAnIndentedTreeWithConfidenceAndLocations()` - _(no documentation)_
- `public Task Chain_View_ShowsInferredLinksWithTheirReason()` - _(no documentation)_
- `public Task Chain_ViewModelAndCommand_PrintTheirTrees()` - _(no documentation)_
- `public Task Chain_Json_IsAValidTreeWithTheSameSections()` - _(no documentation)_
- `public Task Chain_AmbiguousUnknownAndNotSupported_ExitWithOne()` - _(no documentation)_
- `public Task Chain_JsonForAProblem_IsValidJsonWithTheStatus()` - _(no documentation)_
- `public Task Chain_BadDepth_IsAUsageError(params string[] depth)` - _(no documentation)_
- `public Task Chain_Depth_ExpandsChildViews()` - _(no documentation)_
- `public Task Chain_WithoutAnArgument_IsAUsageError()` - _(no documentation)_
- `public Task Chain_WithoutAnIndex_SaysHowToCreateIt()` - _(no documentation)_
- `public Task Help_ListsChainAndTheDepthOption()` - _(no documentation)_
- `public Task Chain_DoesNotCompileTheSolution()` - _(no documentation)_

## CliXamlTests (class)

`public class CliXamlTests`

Source: `tests/PressToShape.CodeIndex.Tests/Xaml/CliXamlTests.cs:16-266`

_(no documentation)_

**Members**

- `public Task Show_ViewClass_PrintsItsXamlFileBindingsAndHandlers()` - _(no documentation)_
- `public Task Show_ViewClass_PrintsTheEvidenceOfItsViewModelLinks()` - _(no documentation)_
- `public Task Show_Element_PrintsItsOwnRecordsOnly()` - _(no documentation)_
- `public Task Show_ResourceAndStyleAndTemplate_PrintTheirRecords()` - _(no documentation)_
- `public Task Show_Json_ContainsTheXamlFileAndIsValid()` - _(no documentation)_
- `public Task Find_PrintsXamlEntriesWithTheirTypes()` - _(no documentation)_
- `public Task Refs_ViewModelProperty_PrintsTheBindingWithFileLineAndConfidence()` - _(no documentation)_
- `public Task Refs_ViewModelOfAnInferredLink_PrintsTheReasonAndTheInferredMark()` - _(no documentation)_
- `public Task Refs_Json_IsAValidArrayWithConfidence()` - _(no documentation)_
- `public Task Generate_PrintsXamlFileCountAndFindingsAsWarnings()` - _(no documentation)_
- `public Task Generate_MalformedXaml_FailsAndWritesNothing()` - _(no documentation)_
- `public Task Check_AfterGenerate_IncludesXamlFilesAndReportsUpToDate()` - _(no documentation)_
- `public Task Check_ChangedXamlSource_IsReportedAsChanged()` - _(no documentation)_
- `public Task Help_ListsTheXamlSymbolTypes()` - _(no documentation)_

## CommandHandlerTests (class)

`public class CommandHandlerTests`

Source: `tests/PressToShape.CodeIndex.Tests/CommandHandlerTests.cs:9-139`

_(no documentation)_

**Members**

- `public void Build_MethodGroupAssignedInAConstructor_GivesAVerifiedEdgeWithFileAndLine()` - _(no documentation)_
- `public void Build_FieldAndPropertyInitializers_GiveEdges()` - _(no documentation)_
- `public void Build_AsyncAndGenericForms_GiveEdges()` - _(no documentation)_
- `public void Build_Lambda_GivesNoEdge()` - _(no documentation)_
- `public void Build_ParameterAndLocal_GiveNoEdge()` - _(no documentation)_
- `public void Build_MethodOfAnotherAssembly_GivesNoEdge()` - _(no documentation)_
- `public void Build_AssignmentToALocal_GivesNoEdge()` - _(no documentation)_
- `public void Build_AnotherCommandType_GivesNoEdge()` - _(no documentation)_
- `public void Build_TwoCommandsInOneConstructor_GiveTwoEdges()` - _(no documentation)_
- `public void FromGenerated_PropertyGeneratedFromARelayCommandMethod_GivesAVerifiedEdgeAtTheMethod()` - _(no documentation)_
- `public void FromGenerated_ObservablePropertyFieldAndHandWrittenMembers_GiveNoEdge()` - _(no documentation)_

## ControlFactsTests (class)

`public class ControlFactsTests`

Source: `tests/PressToShape.CodeIndex.Tests/Avalonia/ControlFactsTests.cs:10-171`

_(no documentation)_

**Members**

- `public void Extract_CustomControl_HasIdBaseTypeSummaryFileAndLine()` - _(no documentation)_
- `public void Extract_ClassesThatAreNotControlsOrAreViews_AreSkipped()` - _(no documentation)_
- `public void Extract_StyledProperty_HasNameTypeDefaultAndWrapper()` - _(no documentation)_
- `public void Extract_StyledPropertyWithoutDefaultAndWrapper_SaysSo()` - _(no documentation)_
- `public void Extract_AttachedProperty_UsesTheStaticGetterAsWrapper()` - _(no documentation)_
- `public void Extract_DirectProperty_UsesTheValueTypeArgument()` - _(no documentation)_
- `public void Extract_NonConstantRegistrationName_FallsBackToTheFieldNameWithoutProperty()` - _(no documentation)_
- `public void Extract_Properties_AreSortedByName()` - _(no documentation)_
- `public void Extract_RoutedEvents_HaveNameArgumentTypeAndRoutingAsWritten()` - _(no documentation)_
- `public void Extract_ControlWithoutPropertiesOrEvents_HasEmptyLists()` - _(no documentation)_
- `public void Extract_ProjectWithoutControls_IsEmptyAndTheOutputIsDeterministic()` - _(no documentation)_

## DataContextAssignmentTests (class)

`public class DataContextAssignmentTests`

Source: `tests/PressToShape.CodeIndex.Tests/DataContextAssignmentTests.cs:9-138`

_(no documentation)_

**Members**

- `public void Build_ObjectInitializer_GivesAVerifiedLinkWithFileAndLine()` - _(no documentation)_
- `public void Build_PlainAssignment_GivesALink()` - _(no documentation)_
- `public void Build_ValueThroughAVariable_UsesItsStaticType()` - _(no documentation)_
- `public void Build_ValueOfASolutionInterfaceType_LinksToTheInterface()` - _(no documentation)_
- `public void Build_ValueOfTypeObject_GivesNoLink()` - _(no documentation)_
- `public void Build_FrameworkValueType_GivesNoLink()` - _(no documentation)_
- `public void Build_TargetThatIsNotAControl_GivesNoLink()` - _(no documentation)_
- `public void Build_OtherPropertyNamedLikeADataContext_GivesNoLink()` - _(no documentation)_
- `public void Build_AssignmentInsideTheViewClass_UsesTheViewClass()` - _(no documentation)_
- `public void Build_AssignmentInALambda_IsFound()` - _(no documentation)_
- `public void Build_SeveralAssignments_AllAppearOnceAndInOrder()` - _(no documentation)_
- `public void Build_NullValue_GivesNoLink()` - _(no documentation)_

## DocCommentReaderTests (class)

`public class DocCommentReaderTests`

Source: `tests/PressToShape.CodeIndex.Tests/DocCommentReaderTests.cs:9-210`

_(no documentation)_

**Members**

- `public void Read_FullComment_StoresEverySection()` - _(no documentation)_
- `public void Read_NoComment_IsUndocumentedWithoutText()` - _(no documentation)_
- `public void Read_OnlySummary_LeavesOtherSectionsEmpty()` - _(no documentation)_
- `public void Read_EmptySummary_CountsAsUndocumented()` - _(no documentation)_
- `public void Read_InheritDocOnOverride_UsesBaseTextAndNamesSource()` - _(no documentation)_
- `public void Read_InheritDocOnInterfaceImplementation_UsesInterfaceText()` - _(no documentation)_
- `public void Read_InheritDocWithOwnSummary_KeepsOwnTextAndFillsTheRest()` - _(no documentation)_
- `public void Read_InheritDocChain_PointsAtTheSymbolThatHasTheText()` - _(no documentation)_
- `public void Read_InheritDocWithoutDocumentedSource_StaysUndocumented()` - _(no documentation)_
- `public void Read_InheritDocWithCref_UsesThatSymbol()` - _(no documentation)_
- `public void Read_InlineTags_AreShownAsPlainText()` - _(no documentation)_
- `public void Read_MalformedXml_DoesNotThrowAndIsUndocumented()` - _(no documentation)_
- `public void Read_ParamsAreKeptInSourceOrder()` - _(no documentation)_

## FixedSourceGenerator (class)

`internal sealed class FixedSourceGenerator : IIncrementalGenerator`

Source: `tests/PressToShape.CodeIndex.Tests/GeneratedMemberLinkerTests.cs:121-125`

_(no documentation)_

**Members**

- `public FixedSourceGenerator(string hintName, string source)` - _(no documentation)_
- `public void Initialize(IncrementalGeneratorInitializationContext context)` - _(no documentation)_

## GeneratedMemberLinkerTests (class)

`public class GeneratedMemberLinkerTests`

Source: `tests/PressToShape.CodeIndex.Tests/GeneratedMemberLinkerTests.cs:9-119`

_(no documentation)_

**Members**

- `public void Extract_GeneratedProperty_IsMarkedAndLinkedToTheObservableField()` - _(no documentation)_
- `public void Extract_PartialPropertyWithGeneratedImplementation_StaysHandWrittenAndKeepsItsAttribute()` - _(no documentation)_
- `public void Extract_GeneratedCommand_IsLinkedToTheRelayCommandMethod()` - _(no documentation)_
- `public void Extract_FieldWithPrefix_MapsToCapitalizedPropertyName()` - _(no documentation)_
- `public void Extract_GeneratedMemberWithoutMatchingSource_HasNoOrigin()` - _(no documentation)_
- `public void Extract_HandWrittenMember_IsNotGenerated()` - _(no documentation)_
- `public void Extract_PartialTypeWithGeneratedPart_PointsAtHandWrittenFile()` - _(no documentation)_
- `public void Extract_GeneratedMembers_NeverContainMachinePaths()` - _(no documentation)_

## InMemoryXamlFileSource (class)

`internal sealed class InMemoryXamlFileSource : IXamlFileSource`

Source: `tests/PressToShape.CodeIndex.Tests/Xaml/InMemoryXamlFileSource.cs:9-24`

A XAML file source over a dictionary of repo-relative paths. Used by the XAML tests, so no disk is involved.

**Members**

- `public InMemoryXamlFileSource Add(string path, string text)` - _(no documentation)_
- `public IReadOnlyList<string> List(string projectDirectory)` - _(no documentation)_
- `public string Read(string repoRelativePath)` - _(no documentation)_

## IndexAnalysisTests (class)

`public class IndexAnalysisTests`

Source: `tests/PressToShape.CodeIndex.Tests/Chain/IndexAnalysisTests.cs:16-237`

_(no documentation)_

**Members**

- `public void ViewModelsOf_View_GivesEvidenceAndConfidence()` - _(no documentation)_
- `public void ViewsOf_ViewModel_GivesTheViewsAndNoneForAnOrphan()` - _(no documentation)_
- `public void Services_ListInterfacesWithImplementationsAndCreationSitesWithoutTests()` - _(no documentation)_
- `public void Services_InterfaceWithoutImplementation_HasNoImplementationsAndAClassDependencyListsItsCreationSites()` - _(no documentation)_
- `public void Models_AreReferencedTypesOfProjectsWithoutViewsAndNotTheServices()` - _(no documentation)_
- `public void Models_TypeOfTheSameProjectIsNotAModel()` - _(no documentation)_
- `public void Commands_ListGeneratedMethodGroupAndBoundCommandsWithTheirHandlers()` - _(no documentation)_
- `public void Commands_AreBoundAtTheXamlPlaces()` - _(no documentation)_
- `public void IsCommand_IsTrueForCommandsAndFalseForOtherMembers()` - _(no documentation)_
- `public void HandlerOf_UnknownId_IsNotStaticallyKnown()` - _(no documentation)_
- `public void TestsFor_GroupsPerTestClassAndIgnoresClassesWithoutTestMethods()` - _(no documentation)_
- `public void Services_IgnoreFakesAndHelpersOfTestProjects()` - _(no documentation)_
- `public void CreationSites_ExcludeTestsAndCoverImplicitAndExplicitConstructors()` - _(no documentation)_
- `public void NameOf_UsesTheSymbolNameOrDescribesXamlIds()` - _(no documentation)_

## IndexGeneratorFactsTests (class)

`public class IndexGeneratorFactsTests`

Source: `tests/PressToShape.CodeIndex.Tests/Avalonia/IndexGeneratorFactsTests.cs:13-136`

_(no documentation)_

**Members**

- `public Task GenerateAsync_MergesFactsOfAllProjectsSorted()` - _(no documentation)_
- `public Task GenerateAsync_ViewWithAXamlFile_IsNotACustomControl()` - _(no documentation)_
- `public Task GenerateAsync_GeneratedCommandAndMethodGroupCommand_GetExecutesMethodEdgesInTheOwningProject()` - _(no documentation)_
- `public Task GenerateAsync_ProjectWithoutAvaloniaTypes_HasEmptyFacts()` - _(no documentation)_
- `public Task GenerateAsync_ShuffledProjects_GiveTheSameFactsAndEdges()` - _(no documentation)_
- `public Task GenerateAsync_FactsAreNotWrittenAsJson()` - _(no documentation)_

## IndexGeneratorTests (class)

`public class IndexGeneratorTests`

Source: `tests/PressToShape.CodeIndex.Tests/IndexGeneratorTests.cs:15-122`

_(no documentation)_

**Members**

- `public Task GenerateAsync_TwoProjects_ExtractsSymbolsPerProjectSortedByName()` - _(no documentation)_
- `public Task GenerateAsync_EdgeAcrossProjects_IsRecordedInTheProjectThatHasTheSource()` - _(no documentation)_
- `public Task GenerateAsync_ErrorProblem_SetsHasErrorsAndKeepsTheOtherProjects()` - _(no documentation)_
- `public Task GenerateAsync_OnlyWarningProblems_DoNotSetHasErrors()` - _(no documentation)_
- `public Task Write_TwoRuns_ProduceIdenticalFilesIncludingTheProblemsInTheManifest()` - _(no documentation)_
- `public Task Write_NoProjects_StillWritesAManifestAndEmptyNames()` - _(no documentation)_
- `public Task GenerateAsync_Cancelled_Throws()` - _(no documentation)_

## IndexGeneratorXamlTests (class)

`public class IndexGeneratorXamlTests`

Source: `tests/PressToShape.CodeIndex.Tests/Xaml/IndexGeneratorXamlTests.cs:16-222`

_(no documentation)_

**Members**

- `public Task GenerateAsync_WithXaml_PutsFilesAndEdgesIntoTheOwningProject()` - _(no documentation)_
- `public Task GenerateAsync_IncludeAndResourceAcrossProjects_AreResolved()` - _(no documentation)_
- `public Task GenerateAsync_CSharpDataContextAssignmentAndStructuralEdges_StillAppear()` - _(no documentation)_
- `public Task GenerateAsync_BindingToAMissingProperty_IsUnresolvedAndMakesNoInferredLink()` - _(no documentation)_
- `public Task GenerateAsync_MalformedXaml_IsAProjectErrorAndOtherFilesAreKept()` - _(no documentation)_
- `public Task GenerateAsync_WithoutAXamlSource_BehavesLikeBefore()` - _(no documentation)_
- `public Task GenerateAsync_ProjectWithoutXamlFiles_IsUnchanged()` - _(no documentation)_
- `public Task GenerateAsync_NoXamlAtAll_GivesEmptyListsAndNoProblems()` - _(no documentation)_
- `public Task GenerateAsync_RegisteredLocatorWithAnUnrecognizedConvention_AddsOneFindingToTheApplicationFile()` - _(no documentation)_
- `public Task GenerateAsync_ApplicationWithoutALocator_HasNoConventionFinding()` - _(no documentation)_
- `public Task GenerateAsync_Twice_GivesIdenticalXamlData()` - _(no documentation)_
- `public Task GenerateAsync_Output_HasNoAbsolutePaths()` - _(no documentation)_

## IndexModelTests (class)

`public class IndexModelTests`

Source: `tests/PressToShape.CodeIndex.Tests/IndexModelTests.cs:10-84`

_(no documentation)_

**Members**

- `public void SymbolRecord_RoundTrip_KeepsAllValues()` - _(no documentation)_
- `public void RelationshipRecord_RoundTrip_WritesEnumsAsNames()` - _(no documentation)_
- `public void IndexManifest_RoundTrip_KeepsProjectsAndWarnings()` - _(no documentation)_
- `public void DocRecord_None_HasNoText()` - _(no documentation)_
- `public void RepoRoot_Find_ReturnsFirstFolderWithSolution()` - _(no documentation)_
- `public void RepoRoot_Find_NoSolution_Throws()` - _(no documentation)_

## IndexSinkTestExtensions (class)

`internal static class IndexSinkTestExtensions`

Source: `tests/PressToShape.CodeIndex.Tests/IndexSinkTestExtensions.cs:5-18`

_(no documentation)_

**Members**

- `public InMemoryFileSink WithReplaced(this InMemoryFileSink source, string path, byte[] content)` - A copy of the index in which one file has other content, to test damaged or old index files. A sink does not allow a second write of a path.

## IndexViewTests (class)

`public class IndexViewTests`

Source: `tests/PressToShape.CodeIndex.Tests/Chain/IndexViewTests.cs:12-153`

_(no documentation)_

**Members**

- `public void Parity_Projects_AreTheSame()` - _(no documentation)_
- `public void Parity_Relationships_AreTheSameSet()` - _(no documentation)_
- `public void Parity_EverySymbolExceptNamespaces_IsFoundWithTheSameRecord()` - _(no documentation)_
- `public void Parity_MembersOfEveryType_AreTheSame()` - _(no documentation)_
- `public void Parity_XamlFiles_AreTheSame()` - _(no documentation)_
- `public void ReaderView_ConstructorSymbol_IsFoundThroughItsType()` - _(no documentation)_
- `public void Views_UnknownIds_GiveNullOrEmpty()` - _(no documentation)_
- `public void ReaderView_ReadsOnlyWhatItNeeds()` - _(no documentation)_
- `public void ReaderView_SingleXamlFile_DoesNotReadTheOthers()` - _(no documentation)_
- `public void ModelView_Output_IsDeterministicForShuffledProjects()` - _(no documentation)_

## JsonIndexWriterTests (class)

`public class JsonIndexWriterTests`

Source: `tests/PressToShape.CodeIndex.Tests/JsonIndexWriterTests.cs:14-203`

_(no documentation)_

**Members**

- `internal IndexData BuildData(bool reverse = false)` - _(no documentation)_
- `public void Write_TwoRuns_ProduceIdenticalBytes()` - _(no documentation)_
- `public void Write_InputInAnotherOrder_ProducesIdenticalBytes()` - _(no documentation)_
- `public void Write_Layout_HasManifestNamesOneFilePerNamespaceAndRelationshipFilePerProject()` - _(no documentation)_
- `public void Write_Manifest_HasSchemaVersionToolVersionCountsAndWarnings()` - _(no documentation)_
- `public void Write_SymbolAndRelationshipFiles_CarrySchemaVersionAndSortedContent()` - _(no documentation)_
- `public void Write_AnyFile_HasNoAbsolutePathsNoMachineNamesNoTimestampsNoBom()` - _(no documentation)_
- `public void Write_Enums_AreNamesAndHtmlCharactersStayReadable()` - _(no documentation)_
- `public void Write_NamesTable_HasOneLinePerTypeAndMemberWithoutNamespacesAndConstructors()` - _(no documentation)_
- `public void Write_LargeFiles_HaveOneRecordPerLineAndStayValidJson()` - _(no documentation)_
- `public void Write_EmptyList_StillWritesValidJson()` - _(no documentation)_
- `public void IndexLayout_SymbolsPath_KeepsNamesInsideTheirFolder(string project, string ns, string expected)` - _(no documentation)_

## LoadPolicyTests (class)

`public class LoadPolicyTests`

Source: `tests/PressToShape.CodeIndex.Tests/IndexGeneratorTests.cs:124-137`

_(no documentation)_

**Members**

- `public void IsIndexed_OnlySrcAndTests(string path, bool expected)` - _(no documentation)_
- `public void IsWarning_OnlyAndroid(string project, bool expected)` - _(no documentation)_

## MarkdownWriterTests (class)

`public class MarkdownWriterTests`

Source: `tests/PressToShape.CodeIndex.Tests/MarkdownWriterTests.cs:10-160`

_(no documentation)_

**Members**

- `public void Write_TwoRunsAndShuffledInput_ProduceIdenticalBytes()` - _(no documentation)_
- `public void Write_Layout_HasReadmeAndOnePagePerNamespaceWithTypes()` - _(no documentation)_
- `public void Write_TypePage_ShowsHeaderSourceSummaryMembersAndRelationships()` - _(no documentation)_
- `public void Write_UndocumentedSymbols_AreMarkedWithoutInventedText()` - _(no documentation)_
- `public void Write_PrivateMembersAndTypes_AreLeftOutButInternalOnesStay()` - _(no documentation)_
- `public void Write_InferredEdges_AreMarked()` - _(no documentation)_
- `public void Write_ChangedSummaryInTheModel_ChangesThePage()` - _(no documentation)_
- `public void Write_AnyPage_HasNoAbsolutePathsNoCarriageReturnsNoBomAndAnotherWordForKind()` - _(no documentation)_
- `public void Write_GeneratedMember_ShowsItsOrigin()` - _(no documentation)_

## MarkupExtensionParserTests (class)

`public class MarkupExtensionParserTests`

Source: `tests/PressToShape.CodeIndex.Tests/Xaml/MarkupExtensionParserTests.cs:8-226`

_(no documentation)_

**Members**

- `public void TryParse_BindingWithPositionalPath_ReadsNameAndPath()` - _(no documentation)_
- `public void TryParse_BindingWithoutArguments_HasNone()` - _(no documentation)_
- `public void TryParse_PositionalAndNamedArguments_AreSeparated()` - _(no documentation)_
- `public void TryParse_NamedPathOnly_ReadsIt()` - _(no documentation)_
- `public void TryParse_NestedExtension_IsKept()` - _(no documentation)_
- `public void TryParse_TwoLevelsOfNesting_Works()` - _(no documentation)_
- `public void TryParse_StaticWithPrefixedMember_ReadsTheMemberText()` - _(no documentation)_
- `public void TryParse_QuotedText_KeepsCommasAndEscapes(string text, string expected)` - _(no documentation)_
- `public void TryParse_OtherExtensions_ReadNameAndKey(string text, string name)` - _(no documentation)_
- `public void TryParse_ParentPathWithCommaInBrackets_DoesNotSplit()` - _(no documentation)_
- `public void TryParse_SpecialPathForms_AreKeptAsText(string text, string path)` - _(no documentation)_
- `public void TryParse_ElementNameAndWhitespaceVariations_Work()` - _(no documentation)_
- `public void TryParse_TrailingComma_IsTolerated()` - _(no documentation)_
- `public void TryParse_BadOrNonExtensionInput_ReturnsFalseWithoutThrowing(string text)` - _(no documentation)_
- `public void TryParse_Null_ReturnsFalse()` - _(no documentation)_
- `public void TryParse_UnknownName_StillParses()` - _(no documentation)_
- `public void TryParse_NestingAtTheLimit_WorksAndBeyondTheLimit_Fails()` - _(no documentation)_
- `public void TryParse_VeryLongInput_ReturnsFalse()` - _(no documentation)_
- `public void TryParse_ManyArguments_AreAllKept()` - _(no documentation)_
- `public void TryParse_Garbage_NeverThrows()` - _(no documentation)_

## MemoryIndexFolder (class)

`internal sealed class MemoryIndexFolder : IIndexFolder`

Source: `tests/PressToShape.CodeIndex.Tests/Chain/MemoryIndexFolder.cs:9-30`

An index folder in memory, with the stale-file removal of the real one, for command line tests.

**Members**

- `public void Write(string relativePath, byte[] content)` - _(no documentation)_
- `public byte[]? Read(string relativePath)` - _(no documentation)_
- `public IReadOnlyList<string> ListFiles()` - _(no documentation)_
- `public IReadOnlyList<string> RemoveStale()` - _(no documentation)_

## MvvmSampleData (class)

`internal static class MvvmSampleData`

Source: `tests/PressToShape.CodeIndex.Tests/Avalonia/MvvmSampleData.cs:15-274`

A small MVVM solution for the chain and page tests: a model project, an app project (views, view models, services, a control, converters, the application class) and a test project. It is built through the real generator, with in-memory XAML.

**Members**

- `public string ToolkitStubs` - _(no documentation)_
- `public string CoreSource` - _(no documentation)_
- `public string AppSource` - _(no documentation)_
- `public string TestSource` - _(no documentation)_
- `public InMemoryXamlFileSource Files()` - _(no documentation)_
- `public ProjectCompilation[] Projects()` - _(no documentation)_
- `public GenerateResult Generate(bool reverse = false)` - _(no documentation)_
- `public IndexData Data(bool reverse = false)` - _(no documentation)_
- `public InMemoryFileSink Index(bool reverse = false)` - _(no documentation)_

## OperationRelationshipTests (class)

`public class OperationRelationshipTests`

Source: `tests/PressToShape.CodeIndex.Tests/OperationRelationshipTests.cs:10-237`

_(no documentation)_

**Members**

- `public void Build_DirectCall_RecordsCallWithLine()` - _(no documentation)_
- `public void Build_CallThroughInterface_PointsAtTheInterfaceMethod()` - _(no documentation)_
- `public void Build_GenericMethodAndExtensionMethod_UseTheDeclaration()` - _(no documentation)_
- `public void Build_MemberReferences_AreRecorded()` - _(no documentation)_
- `public void Build_ObjectCreation_RecordsConstructorCallAndTypeReference()` - _(no documentation)_
- `public void Build_SignatureTypes_AreReferences()` - _(no documentation)_
- `public void Build_FrameworkTargets_AreNotRecorded()` - _(no documentation)_
- `public void Build_DynamicAndDelegateCalls_AreNotRecordedAsCalls()` - _(no documentation)_
- `public void Build_LambdaAndLocalFunction_AreAttributedToTheEnclosingMember()` - _(no documentation)_
- `public void Build_AccessorsInitializersAndConstructorChaining_AreAttributed()` - _(no documentation)_
- `public void Build_NoContainer_NoRegistrationIsInvented()` - _(no documentation)_
- `public void Build_ContainerRegistration_RecordsServiceToImplementation()` - _(no documentation)_
- `public void Build_AllEdges_AreVerifiedAndSorted()` - _(no documentation)_

## PathCollisionGuardTests (class)

`public sealed class PathCollisionGuardTests : IDisposable`

Source: `tests/PressToShape.CodeIndex.Tests/PathCollisionGuardTests.cs:11-104`

Two source files must never produce the same index file, also not on a file system that ignores the case of names.

**Members**

- `public void Dispose()` - _(no documentation)_
- `public void Register_DifferentPaths_Pass()` - _(no documentation)_
- `public void Register_TheSamePathTwice_ThrowsAndNamesThePath()` - _(no documentation)_
- `public void Register_PathThatDiffersOnlyInCase_ThrowsAndNamesBothPaths()` - _(no documentation)_
- `public void InMemoryFileSink_SecondWriteOfAPath_ThrowsAndKeepsTheFirstContent()` - _(no documentation)_
- `public void InMemoryFileSink_PathThatDiffersOnlyInCase_Throws()` - _(no documentation)_
- `public void AtomicFileWriter_SecondWriteOfAPath_ThrowsAndKeepsTheFirstFile()` - _(no documentation)_
- `public void AtomicFileWriter_PathThatDiffersOnlyInCase_ThrowsBeforeTouchingTheDisk()` - _(no documentation)_

## PathScrubberTests (class)

`public class PathScrubberTests`

Source: `tests/PressToShape.CodeIndex.Tests/IndexGeneratorTests.cs:139-163`

_(no documentation)_

**Members**

- `public void Scrub_RepoRootAndHome_AreReplaced()` - _(no documentation)_
- `public void Scrub_WindowsStyleRoot_IsReplaced()` - _(no documentation)_
- `public void Scrub_NoHomeDirectory_LeavesOtherText()` - _(no documentation)_

## ProjectReferenceTests (class)

`public class ProjectReferenceTests`

Source: `tests/PressToShape.CodeIndex.Tests/ProjectReferenceTests.cs:9-61`

_(no documentation)_

**Members**

- `public void SrcProjects_References_NoToolOrRoslynOrLocator()` - _(no documentation)_
- `public void AllProjects_PackageReference_HasNoVersionAttribute()` - _(no documentation)_

## QueryServiceTests (class)

`public class QueryServiceTests`

Source: `tests/PressToShape.CodeIndex.Tests/QueryServiceTests.cs:16-295`

_(no documentation)_

**Members**

- `public void Search_ExactMatches_AreOrderedById()` - _(no documentation)_
- `public void Search_ExactBeforePrefixBeforeSubstring_AndCaseInsensitiveAfterExact()` - _(no documentation)_
- `public void Search_TermWithDot_AlsoMatchesIdsAndRespectsLimit()` - _(no documentation)_
- `public void Show_UniqueType_ReturnsSymbolWithDocsAndRelationships()` - _(no documentation)_
- `public void Show_ById_ResolvesAMember()` - _(no documentation)_
- `public void Show_TypeWithSameShortNameAsFullName_ResolvesWithoutPrefix()` - _(no documentation)_
- `public void Show_AmbiguousName_ListsCandidatesAndNoValue()` - _(no documentation)_
- `public void Show_UnknownName_IsNotFound()` - _(no documentation)_
- `public void Implementations_Interface_ListsVerifiedImplementersAndExtendingInterfaces()` - _(no documentation)_
- `public void Implementations_InterfaceMethod_ListsMatchingMembersAsInferred()` - _(no documentation)_
- `public void Derived_Class_ListsDirectSubtypes()` - _(no documentation)_
- `public void References_Method_ListsVerifiedCallsAndInferredCallsThroughTheInterface()` - _(no documentation)_
- `public void References_OverrideMethod_AlsoCountsCallsOfTheBaseMember()` - _(no documentation)_
- `public void References_Type_ListsSignatureAndCreationReferences()` - _(no documentation)_
- `public void Dependencies_Type_ListsConstructorDependencies()` - _(no documentation)_
- `public void Queries_UnknownSymbol_AreNotFoundForEveryQuery()` - _(no documentation)_
- `public void Search_ReadsOnlyTheManifestAndTheNamesTable()` - _(no documentation)_
- `public void Show_ReadsOneSymbolFileNotTheWholeIndex()` - _(no documentation)_
- `public void Reader_MissingIndex_SaysHowToCreateIt()` - _(no documentation)_
- `public void Reader_OtherSchemaVersion_SaysHowToRegenerate()` - _(no documentation)_
- `public void Reader_DamagedNamesTable_IsReportedNotCrashed()` - _(no documentation)_
- `public void Reader_InvalidJson_IsReportedNotCrashed()` - _(no documentation)_
- `public void DirectoryFileSource_MissingFile_ReturnsNullAndRejectsOutsidePaths()` - _(no documentation)_

## RelationshipBuilderTests (class)

`public class RelationshipBuilderTests`

Source: `tests/PressToShape.CodeIndex.Tests/RelationshipBuilderTests.cs:11-153`

_(no documentation)_

**Members**

- `public void Build_ClassChain_RecordsInheritsForEachLevel()` - _(no documentation)_
- `public void Build_InterfaceImplementation_RecordsImplementsIncludingInheritedInterfaces()` - _(no documentation)_
- `public void Build_FrameworkTypes_AreNeverTargets()` - _(no documentation)_
- `public void Build_ConstructorParameters_RecordInjectedDependencies()` - _(no documentation)_
- `public void Build_ExplicitConstructor_RecordsInjectedDependency()` - _(no documentation)_
- `public void Build_TwoConstructorsSameParameter_GiveOneEdge()` - _(no documentation)_
- `public void Build_GenericBaseAndParameter_UseTheOriginalDefinition()` - _(no documentation)_
- `public void Build_RecordCopyConstructorAndSelfReference_AreIgnored()` - _(no documentation)_
- `public void Build_NestedTypes_AreIncluded()` - _(no documentation)_
- `public void Build_TargetInAnotherProject_IsRecorded()` - _(no documentation)_
- `public void Build_Output_IsSortedAndStable()` - _(no documentation)_

## RepoPaths (class)

`internal static class RepoPaths`

Source: `tests/PressToShape.CodeIndex.Tests/RepoPaths.cs:7-28`

Finds files in the repository, relative to the solution root, without hardcoded absolute paths.

**Members**

- `public string Root { get; }` - _(no documentation)_
- `public string Combine(string relativePath)` - _(no documentation)_

## ResourceResolverTests (class)

`public class ResourceResolverTests`

Source: `tests/PressToShape.CodeIndex.Tests/Xaml/ResourceResolverTests.cs:8-339`

_(no documentation)_

**Members**

- `public void Resolve_ElementScope_FindsTheDefinitionOnTheElement()` - _(no documentation)_
- `public void Resolve_AncestorScope_FindsTheNearestAncestorDefinition()` - _(no documentation)_
- `public void Resolve_ViewRootWithResourceDictionaryChild_IsFound()` - _(no documentation)_
- `public void Resolve_ApplicationResources_AreUsedWhenNoElementDefinesTheKey()` - _(no documentation)_
- `public void Resolve_MergedDictionary_IsFoundThroughTheInclude()` - _(no documentation)_
- `public void Resolve_KeyInBothThemeVariants_GivesOneHitPerVariant()` - _(no documentation)_
- `public void Resolve_KeyInOneVariantOnly_GivesOneHit()` - _(no documentation)_
- `public void Resolve_MergedDictionaries_LaterOnesWin()` - _(no documentation)_
- `public void Resolve_OwnEntryBeatsMergedEntryInTheSameDictionary()` - _(no documentation)_
- `public void Resolve_StyleIncludeResources_AreSearchedAfterTheApplicationResources()` - _(no documentation)_
- `public void Resolve_ReferenceInsideAStyleFile_FindsASiblingDefinition()` - _(no documentation)_
- `public void Resolve_IncludeAcrossProjects_UsesTheAssemblyMap()` - _(no documentation)_
- `public void ResolveInclude_RelativeAndAbsoluteInAssemblyPaths_AreResolved()` - _(no documentation)_
- `public void ResolveInclude_OutsideTheSolution_IsNull(string source)` - _(no documentation)_
- `public void Resolve_NearestWins_ElementBeforeApplication()` - _(no documentation)_
- `public void Resolve_UnknownKey_IsNotFoundWithTheThemeReason()` - _(no documentation)_
- `public void Resolve_NoApplicationDocument_StillSearchesTheViewScopes()` - _(no documentation)_
- `public void Resolve_IncludeCycle_TerminatesWithoutAHit()` - _(no documentation)_
- `public void Resolve_IncludeOutsideTheSolution_IsIgnored()` - _(no documentation)_
- `public void Definitions_ListsEveryEntryWithIdsAndVariants()` - _(no documentation)_
- `public void IncludesOf_ListsStyleAndResourceIncludesWithTargets()` - _(no documentation)_
- `public void ApplicationFor_PrefersTheApplicationOfTheSameProject()` - _(no documentation)_

## RuleFileTests (class)

`public class RuleFileTests`

Source: `tests/PressToShape.CodeIndex.Tests/RuleFileTests.cs:11-91`

The agent rule file is documentation that must stay true: it names real commands and is scoped to the UI files. These tests read it from the repository.

**Members**

- `public void Rule_HasPathsFrontmatterForTheUiFiles()` - _(no documentation)_
- `public void Rule_NamesTheCommandsAndEachOneIsInTheHelpText()` - _(no documentation)_
- `public void Rule_NamesOnlyCommandsThatExist()` - _(no documentation)_
- `public void Rule_HasTheSevenStepsInOrder()` - _(no documentation)_
- `public void Rule_ForbidsAssumingFromNamesAndExplainsInferredLinks()` - _(no documentation)_
- `public void Rule_HasNoAbsolutePathsAndNoWordKind()` - _(no documentation)_
- `public void ProjectOverview_PointsToTheRuleAndReadmeDescribesChain()` - _(no documentation)_

## SampleSolutionTests (class)

`public class SampleSolutionTests`

Source: `tests/PressToShape.CodeIndex.Tests/Avalonia/SampleSolutionTests.cs:7-69`

_(no documentation)_

**Members**

- `public void Generate_Sample_HasNoErrorsAndThreeProjects()` - _(no documentation)_
- `public void Generate_Sample_HasTheAvaloniaFactsThePagesNeed()` - _(no documentation)_
- `public void Generate_Sample_HasHandlerEdgesForGeneratedAndMethodGroupCommandsOnly()` - _(no documentation)_
- `public void Generate_Sample_HasVerifiedAndInferredViewModelLinks()` - _(no documentation)_
- `public void Generate_Sample_HasTestClassesThatReferenceTheViewModelAndTheService()` - _(no documentation)_
- `public void Generate_Sample_IsDeterministicForShuffledProjects()` - _(no documentation)_

## SymbolExtractorTests (class)

`public class SymbolExtractorTests`

Source: `tests/PressToShape.CodeIndex.Tests/SymbolExtractorTests.cs:9-254`

_(no documentation)_

**Members**

- `public void Extract_EachSymbolType_IsRecognized()` - _(no documentation)_
- `public void Extract_Method_RecordsSignatureParametersAndReturnType()` - _(no documentation)_
- `public void Extract_Property_RecordsTypeAndSignature()` - _(no documentation)_
- `public void Extract_Delegate_TakesParametersFromInvoke()` - _(no documentation)_
- `public void Extract_Modifiers_AreSortedAndStaticClassIsNotAbstractSealed()` - _(no documentation)_
- `public void Extract_InterfaceAndItsMembers_AreNotMarkedAbstract()` - _(no documentation)_
- `public void Extract_Accessibility_UsesCSharpWords()` - _(no documentation)_
- `public void Extract_BaseTypeAndInterfaces_AreRecorded()` - _(no documentation)_
- `public void Extract_Attributes_RecordNameAndArguments()` - _(no documentation)_
- `public void Extract_NestedAndGenericTypes_AreLinkedToTheirContainer()` - _(no documentation)_
- `public void Extract_PartialType_AppearsOnceAtItsFirstDeclaration()` - _(no documentation)_
- `public void Extract_PrimaryConstructor_IsListedWithItsParameters()` - _(no documentation)_
- `public void Extract_ImplicitMembers_AreSkipped()` - _(no documentation)_
- `public void Extract_Locations_AreRepoRelativeWithForwardSlashesAndLineRanges()` - _(no documentation)_
- `public void Extract_Namespaces_AreListedWithParents()` - _(no documentation)_
- `public void Extract_Output_IsSortedAndStable()` - _(no documentation)_

## TestCompilation (class)

`internal static class TestCompilation`

Source: `tests/PressToShape.CodeIndex.Tests/TestCompilation.cs:12-51`

Builds small compilations in memory, so tests need no solution and no MSBuild.

**Members**

- `public string RepoRoot { get; }` - A fake repository root. Nothing is read from or written to it.
- `public ProjectCompilation Project(string name, params string[] sources)` - _(no documentation)_
- `public ProjectCompilation WithGenerator(ProjectCompilation project, IIncrementalGenerator generator)` - Runs a generator over the project and returns the project with the generated trees marked.
- `public ProjectCompilation Project(params string[] sources)` - _(no documentation)_

## ViewLocatorConventionTests (class)

`public class ViewLocatorConventionTests`

Source: `tests/PressToShape.CodeIndex.Tests/Xaml/ViewLocatorConventionTests.cs:9-250`

_(no documentation)_

**Members**

- `public void Detect_TheRealViewLocatorPattern_IsRecognized()` - _(no documentation)_
- `public void Detect_AnotherReplacePair_IsReadFromTheCodeNotAssumed()` - _(no documentation)_
- `public void Detect_ReplaceResultPassedDirectlyToGetType_IsRecognized()` - _(no documentation)_
- `public void Detect_LocatorWithoutReplace_GivesAReason()` - _(no documentation)_
- `public void Detect_ReplaceWithoutGetType_GivesAReason()` - _(no documentation)_
- `public void Detect_ReplaceOnSomethingElse_IsNotRecognized()` - _(no documentation)_
- `public void Detect_GetTypeOfAnotherString_IsNotRecognized()` - _(no documentation)_
- `public void Detect_NoRegistration_GivesAReason()` - _(no documentation)_
- `public void Detect_RegistrationOfATypeThatIsNotALocator_GivesAReason()` - _(no documentation)_
- `public void Detect_LocatorNotImplementingTheInterface_IsSkipped()` - _(no documentation)_
- `public void Detect_SecondRegistrationIsTheLocator_IsFound()` - _(no documentation)_
- `public void ViewFor_ExistingCandidate_IsReturned()` - _(no documentation)_
- `public void ViewFor_NoSuchView_IsNull()` - _(no documentation)_
- `public void ViewFor_NestedViewModel_UsesThePlusSeparatedRuntimeName()` - _(no documentation)_
- `public void ViewFor_NameWithoutTheFindText_IsNull()` - _(no documentation)_
- `public void ViewFor_CandidateInAnotherAssembly_IsNullBecauseTypeGetTypeOnlySearchesTheCallingAssembly()` - _(no documentation)_
- `public void Describe_NamesTheRegistrationTheConventionAndTheEvidence()` - _(no documentation)_

## ViewModelLinkTests (class)

`public class ViewModelLinkTests`

Source: `tests/PressToShape.CodeIndex.Tests/Xaml/ViewModelLinkTests.cs:7-288`

_(no documentation)_

**Members**

- `public void Resolve_XClass_GivesACodeBehindLinkFromTheFileToTheClass()` - _(no documentation)_
- `public void Resolve_FileRecord_HasClassDataTypeAndIds()` - _(no documentation)_
- `public void Resolve_Elements_HaveIdsParentsTypesAndNames()` - _(no documentation)_
- `public void Resolve_XDataType_IsAVerifiedViewModelLink()` - _(no documentation)_
- `public void Resolve_DataContextBindingOnAChildView_IsAVerifiedLink()` - _(no documentation)_
- `public void Resolve_DataContextBindingThatDoesNotResolve_GivesNoLink()` - _(no documentation)_
- `public void Resolve_DataContextBindingWithoutADataType_GivesNoLink()` - _(no documentation)_
- `public void Resolve_DataContextBindingOnANonViewElement_GivesNoLink()` - _(no documentation)_
- `public void Resolve_DesignTimeDataContext_IsInferredAndSaysSo()` - _(no documentation)_
- `public void Resolve_DesignInstanceAttribute_IsAlsoDesignTimeOnly()` - _(no documentation)_
- `public void Resolve_DataTemplateDataType_IsAVerifiedUsesTypeLink()` - _(no documentation)_
- `public void Resolve_ContentControlContentBinding_GivesAnInferredViewLocatorLink()` - _(no documentation)_
- `public void Resolve_ItemsControlWithoutATemplate_LinksTheElementTypeThroughTheLocator()` - _(no documentation)_
- `public void Resolve_ItemsControlWithAnItemTemplate_GetsNoInferredLink()` - _(no documentation)_
- `public void Resolve_ContentControlWithAContentTemplate_GetsNoInferredLink()` - _(no documentation)_
- `public void Resolve_ViewModelWithoutAViewByTheConvention_GetsNoLink()` - _(no documentation)_
- `public void Resolve_NoRecognizedConvention_GivesNoInferredLinksAndNoError()` - _(no documentation)_
- `public void Resolve_MatchingNamesWithoutEvidence_GiveNoLink()` - _(no documentation)_
- `public void Resolve_MissingXClassClass_GivesAFindingAndNoCodeBehindLink()` - _(no documentation)_
- `public void Resolve_DuplicateNames_AreCopiedAsFindings()` - _(no documentation)_
- `public void Resolve_SameInputTwice_GivesTheSameEdgesInTheSameOrder()` - _(no documentation)_

## XamlBindingTests (class)

`public class XamlBindingTests`

Source: `tests/PressToShape.CodeIndex.Tests/Xaml/XamlBindingTests.cs:10-416`

_(no documentation)_

**Members**

- `public void Collect_SimpleBinding_IsVerifiedWithAnEdgeToTheProperty()` - _(no documentation)_
- `public void Collect_NestedPath_ResolvesAndReportsTheTypeId()` - _(no documentation)_
- `public void Collect_PathToASolutionClass_RecordsTheTargetTypeId()` - _(no documentation)_
- `public void Collect_CommandBinding_IsMarkedAsCommandAndKeyBindingsCountToo()` - _(no documentation)_
- `public void Collect_DataTemplateDataType_ScopesTheBindingsInside()` - _(no documentation)_
- `public void Collect_XDataTypeOnATemplate_ScopesTheBindingsInside()` - _(no documentation)_
- `public void Collect_NoDataType_IsUnresolvedWithTheReasonAndNoEdge()` - _(no documentation)_
- `public void Collect_UnresolvableDataType_SaysSo()` - _(no documentation)_
- `public void Collect_MissingSegmentWithADataType_IsUnresolvedAndAFinding()` - _(no documentation)_
- `public void Collect_UnmodelledPathForm_IsUnresolvedButNotAFinding()` - _(no documentation)_
- `public void Collect_ElementName_ResolvesAgainstTheNamedElementType()` - _(no documentation)_
- `public void Collect_ElementNameThatDoesNotExist_IsUnresolved()` - _(no documentation)_
- `public void Collect_RelativeSourceAncestorType_ResolvesAgainstThatType()` - _(no documentation)_
- `public void Collect_ParentSourceWithAType_ResolvesAndWithoutOneDoesNot()` - _(no documentation)_
- `public void Collect_ReflectionBinding_IsOnlyInferredEvenWhenThePathMatches()` - _(no documentation)_
- `public void Collect_TemplateBinding_IsUnresolvedWithAReason()` - _(no documentation)_
- `public void Collect_SourceBinding_IsUnresolved()` - _(no documentation)_
- `public void Collect_SetterValueBinding_IsAttributedToTheSetterProperty()` - _(no documentation)_
- `public void Collect_DataContextBinding_IsRecordedLikeAnyBinding()` - _(no documentation)_
- `public void Collect_ModeAndPathNamedArguments_AreRecorded()` - _(no documentation)_
- `public void Collect_DotPath_ResolvesToTheDataContextWithoutAProperty()` - _(no documentation)_
- `public void Collect_LocalizedTextMarkup_IsNotABinding()` - _(no documentation)_
- `public void Collect_MarkupThatCannotBeRead_IsAFinding()` - _(no documentation)_
- `public void Collect_ConverterByStaticResource_LinksTheConverterClass()` - _(no documentation)_
- `public void Collect_ConverterByXStatic_LinksTheClassWhenTheMemberExists()` - _(no documentation)_
- `public void Collect_ConverterResourceThatIsNotASolutionClass_IsAFinding()` - _(no documentation)_
- `public void Collect_ResourceReferenceFound_IsVerifiedWithAnEdge()` - _(no documentation)_
- `public void Collect_ResourceReferenceNotFound_IsRecordedWithTheThemeReasonAndNoEdge()` - _(no documentation)_
- `public void Collect_ResourceReferenceKeyedByAType_IsRecordedWithTheTypeKeyAndNoFinding()` - _(no documentation)_
- `public void Collect_ResourceReferenceKeyedByATypeThatIsDefinedLocally_IsFound()` - _(no documentation)_
- `public void Collect_ResourceReferenceInASetter_IsAttributedToTheSetterProperty()` - _(no documentation)_
- `public void Collect_ResourceInBothThemeVariants_GivesOneEdgePerVariant()` - _(no documentation)_
- `public void Collect_Output_IsDeterministic()` - _(no documentation)_

## XamlJsonWriterTests (class)

`public class XamlJsonWriterTests`

Source: `tests/PressToShape.CodeIndex.Tests/Xaml/XamlJsonWriterTests.cs:12-159`

_(no documentation)_

**Members**

- `public void Write_OneJsonFilePerXamlFileUnderTheProjectFolder()` - _(no documentation)_
- `public void Write_TwoRunsAndShuffledProjects_GiveIdenticalBytes()` - _(no documentation)_
- `public void Write_FileReadsBackAsTheRecordAndHasTheSchemaVersion()` - _(no documentation)_
- `public void Write_EveryListItemIsOnItsOwnValidJsonLine()` - _(no documentation)_
- `public void Write_ListsAreSortedByLineAndEmptyListsAreWrittenAsEmptyArrays()` - _(no documentation)_
- `public void Write_AnyXamlFile_HasNoAbsolutePathsNoCarriageReturnsAndNoBom()` - _(no documentation)_
- `public void Write_NamesTable_HasEntriesForViewsElementsResourcesStylesAndTemplates()` - _(no documentation)_
- `public void Write_NamesTable_StaysSortedAndKeepsCSharpEntries()` - _(no documentation)_
- `public void Write_Manifest_CountsXamlFilesPerProject()` - _(no documentation)_
- `public void XamlPath_IsRelativeToTheProjectFolderAndSafe(string project, string directory, string file, string expected)` - _(no documentation)_
- `public void Write_ProjectWithoutXaml_WritesNoXamlFiles()` - _(no documentation)_

## XamlMarkdownWriterTests (class)

`public class XamlMarkdownWriterTests`

Source: `tests/PressToShape.CodeIndex.Tests/Xaml/XamlMarkdownWriterTests.cs:11-172`

_(no documentation)_

**Members**

- `public void Write_OnePagePerXamlFileUnderTheXamlFolder()` - _(no documentation)_
- `public void Write_Header_NamesFileRootAndClass()` - _(no documentation)_
- `public void Write_ViewModelLinks_ShowEvidenceAndConfidence()` - _(no documentation)_
- `public void Write_InferredViewLink_IsMarkedInferredWithTheViewLocatorReason()` - _(no documentation)_
- `public void Write_Bindings_ShowTargetsConvertersCommandsAndUnresolvedReasons()` - _(no documentation)_
- `public void Write_UnresolvedBinding_SaysWhy()` - _(no documentation)_
- `public void Write_ResourceReferences_ShowTargetsAndTheNotFoundReason()` - _(no documentation)_
- `public void Write_Definitions_ListResourcesStylesTemplatesAndHandlers()` - _(no documentation)_
- `public void Write_ThemeVariantResources_AreMarked()` - _(no documentation)_
- `public void Write_ReadmeLinksTheXamlPages()` - _(no documentation)_
- `public void Write_Pages_AreDeterministicAndHaveNoAbsolutePathsOrCarriageReturns()` - _(no documentation)_
- `public void Write_Findings_AreListedWhenPresent()` - _(no documentation)_
- `public void XamlMarkdownPath_FollowsTheJsonName()` - _(no documentation)_

## XamlModelTests (class)

`public class XamlModelTests`

Source: `tests/PressToShape.CodeIndex.Tests/Xaml/XamlModelTests.cs:12-111`

_(no documentation)_

**Members**

- `public void XamlFileRecord_RoundTrip_KeepsEveryList()` - _(no documentation)_
- `public void RelationshipType_XamlValues_AreAppendedAfterTheEarlierOnes()` - _(no documentation)_
- `public void IndexSchema_Version_IsThree()` - _(no documentation)_
- `public void ManifestProject_WithoutXamlCount_DefaultsToZero()` - _(no documentation)_
- `public void ProjectIndex_WithoutXaml_HasAnEmptyList()` - _(no documentation)_
- `public void Ids_AreBuiltFromRepoRelativePathsOnly()` - _(no documentation)_
- `public void IsXaml_SeparatesXamlIdsFromRoslynIds(string id, bool expected)` - _(no documentation)_
- `public void Describe_GivesShortReadableText(string id, string expected)` - _(no documentation)_

## XamlQueryTests (class)

`public class XamlQueryTests`

Source: `tests/PressToShape.CodeIndex.Tests/Xaml/XamlQueryTests.cs:12-235`

_(no documentation)_

**Members**

- `public void Search_FindsXamlEntriesTogetherWithCSharpSymbols()` - _(no documentation)_
- `public void Search_FindsNamedElementsResourcesStylesAndTemplates(string term, string symbolType)` - _(no documentation)_
- `public void Show_ViewClass_IncludesItsXamlFileAndTheViewModelLinks()` - _(no documentation)_
- `public void Show_ViewById_ReturnsTheXamlFileAndTheViewClass()` - _(no documentation)_
- `public void Show_XamlWithoutAClass_HasNoSymbol()` - _(no documentation)_
- `public void Show_NamedElement_ListsItsBindingsAndHandlersAsOutgoing()` - _(no documentation)_
- `public void Show_Resource_ListsTheReferencesToItAsIncoming()` - _(no documentation)_
- `public void Show_ResourceInBothThemeVariants_IsAmbiguousWithOneCandidatePerVariant()` - _(no documentation)_
- `public void References_ViewModelProperty_IncludesTheBindingWithFileAndLine()` - _(no documentation)_
- `public void References_Command_IncludesTheBinding()` - _(no documentation)_
- `public void References_ConverterClass_IncludesTheBindingAndTheResourceElement()` - _(no documentation)_
- `public void References_EventHandlerMethod_IncludesTheXamlEvent()` - _(no documentation)_
- `public void References_ViewModelClass_IncludesTheViewsThatUseIt()` - _(no documentation)_
- `public void References_ViewModelClassOfAnInferredLink_IsMarkedInferredWithTheReason()` - _(no documentation)_
- `public void References_ViewClass_IncludesItsXamlFileThroughCodeBehind()` - _(no documentation)_
- `public void References_ResourceKey_IncludesTheXamlPlacesThatUseIt()` - _(no documentation)_
- `public void Queries_ForXamlEntries_DoNotReadSymbolFiles()` - _(no documentation)_
- `public void Show_OldXamlFileSchema_SaysHowToRegenerate()` - _(no documentation)_
- `public void Show_DamagedXamlFile_IsReportedNotCrashed()` - _(no documentation)_
- `public void Show_CSharpOnlyIndex_StillWorksWithoutXaml()` - _(no documentation)_

## XamlReaderTests (class)

`public sealed class XamlReaderTests : IDisposable`

Source: `tests/PressToShape.CodeIndex.Tests/Xaml/XamlReaderTests.cs:9-321`

_(no documentation)_

**Members**

- `public void Dispose()` - _(no documentation)_
- `public void Read_Sample_BuildsHierarchyWithParentsAndPaths()` - _(no documentation)_
- `public void Read_PropertyElement_HasOwnerAndProperty()` - _(no documentation)_
- `public void Read_Names_AreTakenFromXNameAndName()` - _(no documentation)_
- `public void Read_Lines_AreOneBased()` - _(no documentation)_
- `public void Read_Attributes_KeepMarkupMetadataAndHandlerCandidatesOnly()` - _(no documentation)_
- `public void Read_RootLiterals_AreDroppedExceptMetadata()` - _(no documentation)_
- `public void Read_IdentifierLiteralWithoutPrefixIsACandidateAndNumbersAreNot()` - _(no documentation)_
- `public void Read_AttachedPropertyAttributes_AreNotHandlerCandidates()` - _(no documentation)_
- `public void Read_NamespaceDeclarations_AreOnTheNodeAndResolveThroughAncestors()` - _(no documentation)_
- `public void Read_PrefixedElements_KeepTheirPrefix()` - _(no documentation)_
- `public void Read_MarkupThatCannotBeParsed_IsKeptWithoutParsedValue()` - _(no documentation)_
- `public void Read_EscapedBraces_AreALiteralNotMarkup()` - _(no documentation)_
- `public void Read_DuplicateNames_GiveAFinding()` - _(no documentation)_
- `public void Read_CommentsAndWhitespace_AreIgnored()` - _(no documentation)_
- `public void Read_ByteOrderMark_IsAccepted()` - _(no documentation)_
- `public void Read_MalformedXml_GivesAProblemWithFileAndNoDocument(string text)` - _(no documentation)_
- `public void Read_MalformedXml_ReportsTheLine()` - _(no documentation)_
- `public void Read_DtdWithExternalEntity_IsRejectedAndNothingIsRead()` - _(no documentation)_
- `public void Read_EntityExpansionBomb_IsRejected()` - _(no documentation)_
- `public void Read_DeepNesting_WorksUpToTheLimitAndFailsBeyond()` - _(no documentation)_
- `public void Read_SameInput_GivesSameDocument()` - _(no documentation)_
- `public void InMemoryFileSource_List_ReturnsSortedPathsUnderTheProject()` - _(no documentation)_
- `public void DiskFileSource_List_IsSortedRelativeAndSkipsBinAndObj()` - _(no documentation)_
- `public void DiskFileSource_PathsOutsideTheRepo_AreRejected(string path)` - _(no documentation)_

## XamlSampleData (class)

`internal static class XamlSampleData`

Source: `tests/PressToShape.CodeIndex.Tests/Xaml/XamlSampleData.cs:12-96`

A small solution with XAML, generated through the real generator, for the writer, reader and query tests.

**Members**

- `public string AppSource` - _(no documentation)_
- `public string Application` - _(no documentation)_
- `public string Shell` - _(no documentation)_
- `public InMemoryXamlFileSource Files()` - _(no documentation)_
- `public IndexData Data(bool reverse = false)` - _(no documentation)_
- `public InMemoryFileSink Index(bool reverse = false)` - _(no documentation)_

## XamlStructureTests (class)

`public class XamlStructureTests`

Source: `tests/PressToShape.CodeIndex.Tests/Xaml/XamlStructureTests.cs:9-218`

_(no documentation)_

**Members**

- `public void Collect_ResourceDefinitions_HaveIdsKeysTypesAndLines()` - _(no documentation)_
- `public void Collect_ThemeVariantResources_KeepTheirVariant()` - _(no documentation)_
- `public void Collect_Style_RecordsSelectorKeySettersAndReferencedResourceKeys()` - _(no documentation)_
- `public void Collect_NestedStyleAndStyleWithoutSelector_GetTheirOwnRecords()` - _(no documentation)_
- `public void Collect_Templates_RecordTypeTargetAndKey()` - _(no documentation)_
- `public void Collect_HandlerWithOneMatchingMethod_IsVerified()` - _(no documentation)_
- `public void Collect_EventFromABaseClassAndACustomControl_IsRecognized()` - _(no documentation)_
- `public void Collect_HandlerWithoutAMethod_IsUnresolvedWithAReason()` - _(no documentation)_
- `public void Collect_HandlerWithTwoOverloads_IsAmbiguousAndUnresolved()` - _(no documentation)_
- `public void Collect_HandlerWithoutACodeBehindClass_IsUnresolved()` - _(no documentation)_
- `public void Collect_IdentifierAttributesThatAreNotEvents_AreNotHandlers()` - _(no documentation)_
- `public void Collect_SolutionClassesAsElements_GetUsesTypeEdgesAndAvaloniaTypesDoNot()` - _(no documentation)_
- `public void Collect_Includes_LinkToFilesOfTheSolutionOnly()` - _(no documentation)_
- `public void Collect_Output_IsDeterministicAndFindingsAreEmptyForCleanFiles()` - _(no documentation)_

## XamlTestHost (class)

`internal sealed class XamlTestHost`

Source: `tests/PressToShape.CodeIndex.Tests/Xaml/XamlTestHost.cs:12-141`

Builds a small solution in memory: stub Avalonia types, application C# source, XAML files, and the resolver over them.

**Members**

- `public string Header` - _(no documentation)_
- `public string Avalonia` - _(no documentation)_
- `public Dictionary<string, XamlDocument> Documents { get; }` - _(no documentation)_
- `public XamlWorld World { get; }` - _(no documentation)_
- `public XamlResolver Resolver { get; }` - _(no documentation)_
- `public ProjectCompilation Project { get; }` - _(no documentation)_
- `public ViewLocatorConvention? Convention { get; }` - _(no documentation)_
- `public XamlTestHost Create(string appSource, params (string Path, string Xml)[] files)` - Creates the host. XAML files are given as (path, text). A file with a root Application is the application document.
- `public ProjectCompilation AppProject(string name, params string[] sources)` - An application project that references the stub Avalonia types as metadata, so that they are not indexed as part of the solution.
- `public XamlResolution Resolve(string path)` - _(no documentation)_
- `public string Edge(RelationshipRecord edge)` - _(no documentation)_

## XamlTypeResolverTests (class)

`public class XamlTypeResolverTests`

Source: `tests/PressToShape.CodeIndex.Tests/Xaml/XamlTypeResolverTests.cs:9-179`

_(no documentation)_

**Members**

- `public void Resolve_UsingNamespace_FindsTheSolutionType()` - _(no documentation)_
- `public void Resolve_ClrNamespaceWithAssembly_IgnoresTheAssemblyPart()` - _(no documentation)_
- `public void Resolve_DefaultNamespace_UsesXmlnsDefinitionAttributesOfTheReferencedAssembly()` - _(no documentation)_
- `public void Resolve_PackageType_HasNoSolutionIdAndKeepsItsName()` - _(no documentation)_
- `public void Resolve_UnknownPrefix_IsNotResolvedAndKeepsTheText()` - _(no documentation)_
- `public void Resolve_UnknownTypeOrNamespace_IsNotResolved(string text)` - _(no documentation)_
- `public void Resolve_GenericTypeNameWithoutArity_IsNotResolved()` - _(no documentation)_
- `public void Resolve_XamlLanguageNamespaceType_IsNotResolved()` - _(no documentation)_
- `public void Resolve_XmlnsDefinitionInTheSolutionItself_IsUsed()` - _(no documentation)_
- `public void ResolveMember_AttachedPropertyAndStaticMember_SplitOwnerAndMember()` - _(no documentation)_
- `public void ResolveMember_UnknownOwner_IsNotResolvedButKeepsTheMember()` - _(no documentation)_
- `public void Resolve_NodeWithItsOwnNamespaceDeclaration_UsesIt()` - _(no documentation)_

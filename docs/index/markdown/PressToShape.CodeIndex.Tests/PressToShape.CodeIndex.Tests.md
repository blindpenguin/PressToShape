# PressToShape.CodeIndex.Tests

Project: PressToShape.CodeIndex.Tests

## AtomicFileWriterTests (class)

`public sealed class AtomicFileWriterTests : IDisposable`

Source: `tests/PressToShape.CodeIndex.Tests/AtomicFileWriterTests.cs:11-121`

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

## CliAppTests (class)

`public class CliAppTests`

Source: `tests/PressToShape.CodeIndex.Tests/CliAppTests.cs:18-391`

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

Source: `tests/PressToShape.CodeIndex.Tests/MarkdownWriterTests.cs:10-149`

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

Source: `tests/PressToShape.CodeIndex.Tests/QueryServiceTests.cs:16-297`

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

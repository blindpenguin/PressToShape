# PressToShape.Core.Tests.History

Project: PressToShape.Core.Tests

## DocumentHistoryTests (class)

`public class DocumentHistoryTests`

Source: `tests/PressToShape.Core.Tests/History/DocumentHistoryTests.cs:11-428`

_(no documentation)_

**Members**

- `public void New_StartsAtTheGivenDocumentWithoutSteps()` - _(no documentation)_
- `public void Record_NewDocument_BecomesCurrentAndAddsTheStep()` - _(no documentation)_
- `public void Record_TheCurrentInstance_IsNotRecorded()` - _(no documentation)_
- `public void Record_AnEqualButDifferentInstance_IsRecorded()` - _(no documentation)_
- `public void UndoAndRedo_WalkBackAndForth()` - _(no documentation)_
- `public void Redo_AfterUndo_ReturnsTheSameInstance()` - _(no documentation)_
- `public void Record_AfterUndo_DropsTheRedoSteps()` - _(no documentation)_
- `public void Record_OverTheLimit_AllowsExactlyTheLimitOfUndosAndDropsTheOldest()` - _(no documentation)_
- `public void Record_OverTheLimit_AfterUndo_KeepsIndexAndStepsConsistent()` - _(no documentation)_
- `public void MaxSteps_One_KeepsOnlyTheLastStep()` - _(no documentation)_
- `public void JumpTo_ForwardBackAndToTheCurrentIndex()` - _(no documentation)_
- `public void JumpTo_OutOfRange_ReturnsNullAndChangesNothing(int index)` - _(no documentation)_
- `public void Reset_ForgetsEverything()` - _(no documentation)_
- `public void Steps_IsALiveReadOnlyView()` - _(no documentation)_
- `public void Arguments_Invalid_Throw()` - _(no documentation)_
- `public void OriginalDocuments_AreNeverChanged()` - _(no documentation)_
- `public void RandomCalls_NeverThrow_AndCurrentIsAlwaysARecordedDocument()` - _(no documentation)_
- `public void ManyShapes_RecordingIsCheap_BecauseDocumentsShareLayers()` - _(no documentation)_
- `public void RecordOrExtend_FirstStep_IsRecorded()` - _(no documentation)_
- `public void RecordOrExtend_SameTypeAgain_ExtendsTheLastStep()` - _(no documentation)_
- `public void RecordOrExtend_DifferentType_RecordsANewStep()` - _(no documentation)_
- `public void RecordOrExtend_AfterUndo_RecordsANewStepAndDropsTheRedoSteps()` - _(no documentation)_
- `public void RecordOrExtend_AfterUndoOfTheOnlyStepOfThatType_DoesNotExtendTheEarlierStep()` - _(no documentation)_
- `public void RecordOrExtend_TheCurrentInstance_DoesNothing()` - _(no documentation)_
- `public void RecordOrExtend_KeepsTheLimit()` - _(no documentation)_
- `public void RecordOrExtend_NullDocument_Throws()` - _(no documentation)_

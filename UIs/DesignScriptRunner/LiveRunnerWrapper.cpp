#include "StdAfx.h"

DesignScriptRunner* DesignScriptRunner::create(DesignScriptRunnerCallback* pCallback)
{
    return new LiveRunnerWrapper(pCallback);
}

LiveRunnerWrapper::LiveRunnerWrapper(DesignScriptRunnerCallback* pCallback) 
    : mNodeId(1)
{
    mpCallback = pCallback;
    this->setWrapper(gcnew LiveRunner());
}

LiveRunnerWrapper::~LiveRunnerWrapper()
{
    delete mpCallback;
}

void LiveRunnerWrapper::updateGraph(const wchar_t* codesegment)
{
    SynchronizeData^ syncData = createSyncDataFromCode(WcharToString(codesegment));
    wrapper()->UpdateGraph(syncData);
}

void LiveRunnerWrapper::updateCLInterpreter(const wchar_t* codesegment)
{
    wrapper()->UpdateCmdLineInterpreter(WcharToString(codesegment));
}

void LiveRunnerWrapper::updateGraphAsync(const wchar_t* codesegment)
{
    SynchronizeData^ syncData = createSyncDataFromCode(WcharToString(codesegment));
    wrapper()->BeginUpdateGraph(syncData);
}

void LiveRunnerWrapper::updateCLInterpreterAsync(const wchar_t* codesegment)
{
    wrapper()->BeginUpdateCmdLineInterpreter(WcharToString(codesegment));
}

DesignScriptObject* LiveRunnerWrapper::queryNodeValue(const wchar_t* nodeName)
{
    RuntimeMirror^ mirror = wrapper()->QueryNodeValue(WcharToString(nodeName));
    if(nullptr == mirror)
        return nullptr;

    return new MirrorObjectWrapper(mirror);
}

void LiveRunnerWrapper::queryNodeValuesAsync(const std::list<unsigned int>& nodeIds)
{
    System::Collections::Generic::List<unsigned int>^ list =
        gcnew System::Collections::Generic::List<unsigned int>();

    for(std::list<unsigned int>::const_iterator it = nodeIds.begin(); it != nodeIds.end(); ++it)
    {
        list->Add(*it);
    }

    wrapper()->BeginQueryNodeValue(list);
}

DesignScriptObject* LiveRunnerWrapper::getCoreDump()
{
    //Query value for first node
    RuntimeMirror^ mirror = wrapper()->QueryNodeValue(1);
    if(nullptr == mirror)
        return nullptr;

    System::String^ dump = mirror->GetUtils()->GetCoreDump();
    return new SimpleDSObject(dump);
}

const wchar_t* LiveRunnerWrapper::getCoreDumpCmdLineREPL()
{
    return StringToWchar(wrapper()->GetCoreDump());
}


SynchronizeData^ LiveRunnerWrapper::createSyncDataFromCode(System::String^ codesegment)
{
    SnapshotNode^ node = gcnew SnapshotNode(mNodeId++, SnapshotNodeType::CodeBlock, codesegment);
    SynchronizeData^ syncData = gcnew SynchronizeData();
    syncData->AddedNodes->Add(node);
    return syncData;
}

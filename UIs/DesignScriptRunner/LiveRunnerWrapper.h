#pragma once

#include "WrapperObject.h"

using namespace ProtoScript::Runners;
using namespace GraphToDSCompiler;

class LiveRunnerWrapper : public WrapperObject<LiveRunner, DesignScriptRunner>
{
public:
    LiveRunnerWrapper(DesignScriptRunnerCallback* pCallback);

    virtual ~LiveRunnerWrapper();
    virtual void updateGraph(const wchar_t* codesegment);
    virtual void updateCLInterpreter(const wchar_t* codesegment);
    virtual void updateGraphAsync(const wchar_t* codesegment);
    virtual void updateCLInterpreterAsync(const wchar_t* codesegment);
    virtual DesignScriptObject* queryNodeValue(const wchar_t* nodeName);
    virtual void queryNodeValuesAsync(const std::list<unsigned int>& nodeIds);
    virtual DesignScriptObject* getCoreDump();
    virtual const wchar_t* getCoreDumpCmdLineREPL();
    
private:
    SynchronizeData^ createSyncDataFromCode(System::String^ codesegment);
    DesignScriptRunnerCallback* mpCallback;
    unsigned int mNodeId;
};

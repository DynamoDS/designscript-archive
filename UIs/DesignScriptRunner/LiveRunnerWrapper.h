#pragma once

#include "WrapperObject.h"

using namespace ProtoScript::Runners;
using namespace GraphToDSCompiler;
using namespace ProtoCore::AST::AssociativeAST;
using namespace System::Collections::Generic;

class LiveRunnerWrapper : public WrapperObject<LiveRunner, DesignScriptRunner>
{
public:
    LiveRunnerWrapper(DesignScriptRunnerCallback* pCallback);

    virtual ~LiveRunnerWrapper();
    virtual void updateCLInterpreter(const wchar_t* codesegment);
    virtual void importLibrary(const std::vector<const wchar_t*>& libraries);
    virtual void updateCLInterpreterAsync(const wchar_t* codesegment);
    //virtual DesignScriptObject* queryNodeValue(const wchar_t* nodeName);    
    virtual DesignScriptObject* inspectNodeValue(const wchar_t* nodeName);
    //virtual DesignScriptObject* getCoreDump();
    virtual const wchar_t* getCoreDumpCmdLineREPL();
    virtual void reInitializeLiveRunner();
    virtual GraphNode* buildAst(const wchar_t* type, void* hostInstancePtr, const wchar_t* methodName, const std::vector<void*>& selectionInputs, const std::vector<const wchar_t*>& cmdInputs, const wchar_t* formatString);
    virtual GraphNode* buildArrayNode(const std::vector<const wchar_t*>& arrayInputs);

    virtual void getFunctionArgs(DesignScriptMethod* methodMirror, std::vector<const wchar_t*>& argNames, std::vector<DesignScriptClass*>& argTypes);

    virtual std::vector<DesignScriptObject*>* resetAndImportLibrary(const std::vector<const wchar_t*>& libraries);
    
    void updateGraph(GraphNode* graphNode);
private:
    DesignScriptRunnerCallback* mpCallback;
    unsigned int mNodeId;
};

class GraphNodeWrapper : public WrapperObject<AssociativeNode, GraphNode>
{
public:
    GraphNodeWrapper(const wchar_t* type, void* hostInstancePtr, const wchar_t* methodName, const std::vector<void*>& selectionInputs, const std::vector<const wchar_t*>& cmdInputs, const wchar_t* formatString, ProtoCore::Core^ core);
    GraphNodeWrapper(const std::vector<const wchar_t*>& arrayInputs);

    virtual ~GraphNodeWrapper();
    virtual const wchar_t* getNodeName() const;
    virtual const wchar_t* getCode() const;
public:
    const wchar_t* m_symbolName;
    const wchar_t* m_code;
};


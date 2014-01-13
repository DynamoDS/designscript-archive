#include "StdAfx.h"

using namespace ProtoScript::Runners;
using namespace ProtoCore::AST::AssociativeAST;
using namespace System::Collections::Generic;

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
    if(mpCallback != NULL)
    {
        delete mpCallback;
        mpCallback = NULL;
    }
}

void LiveRunnerWrapper::importLibrary(const std::vector<const wchar_t*>& libraries) 
{
    List<System::String^>^ inputList = gcnew List<System::String^>();
    for(std::vector<const wchar_t*>::const_iterator it = libraries.begin(); it != libraries.end(); ++it)    
    {
        inputList->Add(WcharToString(*it));
    }
    wrapper()->ResetVMAndResyncGraph(inputList);
}

std::vector<DesignScriptObject*>* LiveRunnerWrapper::resetAndImportLibrary(const std::vector<const wchar_t*>& libraries)
{
    List<System::String^>^ inputList = gcnew List<System::String^>();
    for(std::vector<const wchar_t*>::const_iterator it = libraries.begin(); it != libraries.end(); ++it)    
    {
        inputList->Add(WcharToString(*it));
    }
    
    std::vector<DesignScriptObject*>* libMirrors = new std::vector<DesignScriptObject*>();
    List<LibraryMirror^>^ libs = wrapper()->ResetVMAndImportLibrary(inputList);
    for(int i=0; i < libs->Count; ++i)
    {
        libMirrors->push_back(new LibraryMirrorWrapper(libs[i]));
    }
    return libMirrors;
}


void LiveRunnerWrapper::updateCLInterpreter(const wchar_t* codesegment)
{
    wrapper()->UpdateCmdLineInterpreter(WcharToString(codesegment));
}

void LiveRunnerWrapper::updateGraph(GraphNode* graphNode)
{
    //TODO: Move buildAst() to GraphNode class and add a LiveRunner member to it, 
    // add an abstract method to GraphNode that is implemented in GraphNodeWrapper
    // that calls this function - this will avoid the need for a dynamic cast
    GraphNodeWrapper* gNode = dynamic_cast<GraphNodeWrapper*>(graphNode);
    if(gNode != NULL)
    {
        AssociativeNode^ astNode = gNode->wrapper();
        wrapper()->UpdateGraph(astNode);
    }
}

void LiveRunnerWrapper::updateCLInterpreterAsync(const wchar_t* codesegment)
{
    wrapper()->BeginUpdateCmdLineInterpreter(WcharToString(codesegment));
}

//DesignScriptObject* LiveRunnerWrapper::queryNodeValue(const wchar_t* nodeName)
//{
//    RuntimeMirror^ mirror = wrapper()->QueryNodeValue(WcharToString(nodeName));
//    if(nullptr == mirror)
//        return nullptr;
//
//    return new MirrorObjectWrapper(mirror);
//}

DesignScriptObject* LiveRunnerWrapper::inspectNodeValue(const wchar_t* nodeName) 
{
    RuntimeMirror^ mirror = wrapper()->InspectNodeValue(WcharToString(nodeName));
    if(nullptr == mirror)
        return nullptr;

    return new MirrorObjectWrapper(mirror);
}

//DesignScriptObject* LiveRunnerWrapper::getCoreDump()
//{
//    //Query value for first node
//    RuntimeMirror^ mirror = wrapper()->QueryNodeValue(1);
//    if(nullptr == mirror)
//        return nullptr;
//
//    System::String^ dump = mirror->GetUtils()->GetCoreDump();
//    return new SimpleDSObject(dump);
//}

const wchar_t* LiveRunnerWrapper::getCoreDumpCmdLineREPL()
{
    return StringToWchar(wrapper()->GetCoreDump());
}


void LiveRunnerWrapper::reInitializeLiveRunner()
{
    wrapper()->ReInitializeLiveRunner();
}

GraphNode* LiveRunnerWrapper::buildAst(const wchar_t* type, void* hostInstancePtr, const wchar_t* methodName, const std::vector<void*>& selectionInputs, const std::vector<const wchar_t*>& cmdInputs, const wchar_t* formatString)
{
    GraphNodeWrapper* gNode = new GraphNodeWrapper(type, hostInstancePtr, methodName, selectionInputs, cmdInputs, formatString, wrapper()->Core);
    return gNode;
}

GraphNode* LiveRunnerWrapper::buildArrayNode(const std::vector<const wchar_t*>& arrayInputs)
{
    return new GraphNodeWrapper(arrayInputs);
}

void LiveRunnerWrapper::getFunctionArgs(DesignScriptMethod* methodMirror, std::vector<const wchar_t*>& argNames, std::vector<DesignScriptClass*>& argTypes)
{
    MethodMirrorWrapper* mWrapper = dynamic_cast<MethodMirrorWrapper*>(methodMirror);
    if(mWrapper != NULL)
    {
        argNames = mWrapper->getArgumentNames();
        argTypes = mWrapper->getArgumentTypes(wrapper()->Core);
    }
}

GraphNode::GraphNode(const wchar_t* type, void* hostInstancePtr, const wchar_t* methodName, const std::vector<void*> selectionInputs, const std::vector<const wchar_t*> cmdInputs)
{
    this->type = type;
    this->hostInstancePtr = hostInstancePtr;
    this->methodName = methodName;
    
}

GraphNodeWrapper::GraphNodeWrapper(const wchar_t* type, void* hostInstancePtr, const wchar_t* methodName, const std::vector<void*>& selectionInputs, const std::vector<const wchar_t*>& cmdInputs, const wchar_t* formatString, ProtoCore::Core^ core) 
{
    List<System::IntPtr>^ selectionList = gcnew List<System::IntPtr>();
    List<System::String^>^ inputList = gcnew List<System::String^>();
    System::String^ symbolName = nullptr;
    System::String^ code = nullptr;
    
    for(std::vector<void*>::const_iterator it = selectionInputs.begin(); it != selectionInputs.end(); ++it)    
    {
        selectionList->Add((System::IntPtr)*it);
    }

    for(std::vector<const wchar_t*>::const_iterator it = cmdInputs.begin(); it != cmdInputs.end(); ++it)    
    {
        inputList->Add(WcharToString(*it));
    }

    AssociativeNode^ pAssocNode = ProtoCore::ASTCompilerUtils::BuildAST(WcharToString(type), (long int)hostInstancePtr, WcharToString(methodName), selectionList, inputList, WcharToString(formatString), core, symbolName, code);

    this->setWrapper(pAssocNode);
    if(symbolName != nullptr)
        m_symbolName = StringToWchar(symbolName);
    else
        m_symbolName = NULL;

    if(code != nullptr)
        m_code = StringToWchar(code);
    else
        m_code = NULL;
}

GraphNodeWrapper::GraphNodeWrapper(const std::vector<const wchar_t*>& arrayInputs)
{
    List<System::String^>^ inputList = gcnew List<System::String^>();
    for(std::vector<const wchar_t*>::const_iterator it = arrayInputs.begin(); it != arrayInputs.end(); ++it)    
    {
        inputList->Add(WcharToString(*it));
    }

    System::String^ symbolName = nullptr;
    System::String^ code = nullptr;
    AssociativeNode^ pAssocNode = ProtoCore::ASTCompilerUtils::BuildArrayNode(inputList, symbolName, code);

    this->setWrapper(pAssocNode);
    if(symbolName != nullptr)
        m_symbolName = StringToWchar(symbolName);
    else
        m_symbolName = NULL;

    if(code != nullptr)
        m_code = StringToWchar(code);
    else
        m_code = NULL;
}

GraphNodeWrapper::~GraphNodeWrapper()
{
    if(m_symbolName != NULL)
    {
        delete m_symbolName;
        m_symbolName = NULL;
    }

    if(m_code != NULL)
    {
        delete m_code;
        m_code = NULL;
    }
}

const wchar_t* GraphNodeWrapper::getNodeName() const
{
    return m_symbolName;
}

const wchar_t* GraphNodeWrapper::getCode() const
{
    return m_code;
}



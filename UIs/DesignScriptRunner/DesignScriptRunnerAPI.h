#pragma once
#include<list>

// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the DESIGNSCRIPTRUNNER_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// DESIGNSCRIPTRUNNER_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef DESIGNSCRIPTRUNNER_EXPORTS
#define DESIGNSCRIPTRUNNER_API __declspec(dllexport)
#else
#define DESIGNSCRIPTRUNNER_API __declspec(dllimport)
#endif

enum ErrorStatus
{
    eOk,
    eFailed,
    eNotApplicable,
    eNotImplemented,
    eKeyNotFound,
};

class DesignScriptClass;
class DesignScriptObject;
class DesignScriptConfiguration;
class DesignScriptRunnerCallback;
class DesignScriptNode;

/// <summary>
/// DesignScriptRunner class implements the design script graph execution 
/// engine. It provides both synchronous as well as asynchronous methods
/// to update and query data from the executing graph.
/// </summary>
class __declspec(novtable) DesignScriptRunner
{
public:
    /// <summary>
    /// Method to create a new instance of DesignScriptRunner implementation.
    /// This method accepts a callback pointer to provide callback methods for
    /// graph update and node value available events. Client can also pass some
    /// configuration parameters using this callback pointer.
    /// </summary>
    DESIGNSCRIPTRUNNER_API static DesignScriptRunner* create(DesignScriptRunnerCallback* pCallback = nullptr);

    /// <summary>
    /// Virtual Destructor
    /// </summary>
    virtual ~DesignScriptRunner() = 0 {}

    //virtual DesignScriptNode* createCodeBlockNode(const wchar_t* codesegment) = 0;
    //virtual DesignScriptNode* createFunctionNode(const wchar_t* functionName) = 0;

    /// <summary>
    /// Synchronous method to update the graph with given code segment. This 
    /// call waits till the graph is evaluated for the changes due to new code 
    /// segment in the graph. 
    /// </summary>
    virtual void updateGraph(const wchar_t* codesegment) = 0;

    /// <summary>
    /// Synchronous method to update the VM with given code segment. This 
    /// call waits till the live execution finishes for the changes due to new code 
    /// segment from command-line interpreter. 
    /// </summary>
    virtual void updateCLInterpreter(const wchar_t* codesegment) = 0;

    /// <summary>
    /// Asynchronous method to update the graph with given code segment. This 
    /// call queues the given code segment to the graph for evaluation. Once
    /// graph is completely evaluated, GraphUpdateReady method is called on the
    /// passed callback object.
    /// </summary>
    virtual void updateGraphAsync(const wchar_t* codesegment) = 0;

    /// <summary>
    /// Asynchronous method to update the VM with given code segment from the command-line interpreter. This 
    /// call queues the given code segment to the live engine for evaluation. 
    /// </summary>
    virtual void updateCLInterpreterAsync(const wchar_t* codesegment) = 0;
    
    /// <summary>
    /// Queries the value of the given node/variable using it's name.
    /// </summary>
    virtual DesignScriptObject* queryNodeValue(const wchar_t* nodeName) = 0;

    /// <summary>
    /// Sets up asyncronous query for value of a list of nodes.
    /// </summary>
    virtual void queryNodeValuesAsync(const std::list<unsigned int>& nodeIds) = 0;

    /// <summary>
    /// Gets the core dump object, which has values/state of all the objects
    /// in the graph.
    /// </summary>
    virtual DesignScriptObject* getCoreDump() = 0;
    
    /// <summary>
    /// Dumps the current VM state as a formatted string of values
    /// in the graph.
    /// </summary>
    virtual const wchar_t* getCoreDumpCmdLineREPL() = 0;

};


/// <summary>
/// DesignScriptRunnerCallback class implements the callback methods as well as
/// provides configuration parameters.
/// </summary>
class __declspec(novtable) DesignScriptRunnerCallback
{
public:
    /// <summary>
    /// Virtual Destructor
    /// </summary>
    virtual ~DesignScriptRunnerCallback() = 0 {}

    /// <summary>
    /// This method is called from the runner, when the node value is ready for 
    /// query if node value query was scheduled thru an asynchronous call.
    /// </summary>
    virtual void NodeValueReady(unsigned int nodeId, DesignScriptObject* pValue, ErrorStatus status) = 0;
    
    /// <summary>
    /// This method is called from the runner, when the graph evaluation is 
    /// completed after an asynchronous call of graph update.
    /// </summary>
    virtual void GraphUpdateReady(ErrorStatus status, const wchar_t* errorMessage) = 0;

    /// <summary>
    /// This method is called from the runner to get the configuration parameters, 
    /// when the DesignScript graph evaluation engine is initialized.
    /// </summary>
    virtual DesignScriptConfiguration& configuration() = 0;
};

class __declspec(novtable) DesignScriptObject
{
public:
    virtual ~DesignScriptObject() = 0 {}
    virtual bool isKindOf(const DesignScriptClass * aClass) const = 0;
    virtual DesignScriptClass* isA() const = 0;
    virtual bool isEqualTo(const DesignScriptObject * other) const = 0;
    virtual int comparedTo(const DesignScriptObject * other) const  = 0;
    virtual const wchar_t* toString() const = 0;
};

class __declspec(novtable) DesignScriptClass
{
public:
    virtual ~DesignScriptClass() = 0 {}
    virtual const wchar_t* name() = 0;
    virtual DesignScriptClass* parent() = 0;
};

class __declspec(novtable) DesignScriptConfiguration
{
public:
    DESIGNSCRIPTRUNNER_API static DesignScriptConfiguration* create();

    class __declspec(novtable) Iterator
    {
    public:
        virtual ~Iterator() = 0;
        virtual void start() = 0;
        virtual void next() = 0;
        virtual bool done() =0;
        virtual const wchar_t* parameter() = 0;
        virtual DesignScriptObject* value() = 0;
    };

    virtual ~DesignScriptConfiguration() = 0 {}
    virtual DesignScriptObject* getValue(const wchar_t* parameter) = 0;
    virtual DesignScriptObject* setValue(const wchar_t* parameter, DesignScriptObject* pValue) = 0;
    virtual Iterator* newIterator() = 0;
};
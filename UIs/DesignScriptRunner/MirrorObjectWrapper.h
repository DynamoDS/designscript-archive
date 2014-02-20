#pragma once
#include "WrapperObject.h"
#include "StringUtils.h"

using namespace ProtoCore::Mirror;

class SimpleDSObject : public DesignScriptObject
{
public:
    SimpleDSObject();
    SimpleDSObject(System::String^ data);
    virtual ~SimpleDSObject(void);
    virtual bool isKindOf(const DesignScriptClass * aClass) const;
    virtual DesignScriptClass* isA() const;
    virtual bool isEqualTo(const DesignScriptObject * other) const;
    virtual int comparedTo(const DesignScriptObject * other) const;
    virtual const wchar_t* toString() const;

    virtual std::vector<DesignScriptClass*>* getClasses() const;
    
    void setStringData(System::String^ data);
protected:
    virtual System::String^ getData() const;

private:
    StringToWchar* mpStringData;
};

class MirrorObjectWrapper : public WrapperObject<RuntimeMirror, DesignScriptObject>
{
public:
    MirrorObjectWrapper(RuntimeMirror^ mirror);
    //virtual bool isEqualTo(const DesignScriptObject * other) const;
    virtual const wchar_t* toString() const;
    virtual std::vector<DesignScriptClass*>* getClasses() const;

protected:
    virtual System::String^ getData() const;
};

class LibraryMirrorWrapper : public WrapperObject<LibraryMirror, DesignScriptObject>
{
public:
    LibraryMirrorWrapper(LibraryMirror^ libMirror);
    virtual ~LibraryMirrorWrapper() {}

    virtual const wchar_t* toString() const;
    virtual std::vector<DesignScriptClass*>* getClasses() const;
};

class ClassMirrorWrapper : public WrapperObject<ClassMirror, DesignScriptClass>
{
public:
    ClassMirrorWrapper(ClassMirror^ classMirror);
    virtual ~ClassMirrorWrapper() {}

    virtual const wchar_t* name() const;
    virtual DesignScriptClass* parent() const;
    virtual std::vector<DesignScriptMethod*>* getConstructors() const;
    virtual std::vector<DesignScriptMethod*>* getMethods() const;
    virtual std::vector<DesignScriptMethod*>* getOverloads(const wchar_t* methodName) const;
};

class MethodMirrorWrapper : public WrapperObject<MethodMirror, DesignScriptMethod>
{
public:
    MethodMirrorWrapper(MethodMirror^ methodMirror);
    virtual ~MethodMirrorWrapper() {}

    virtual const wchar_t* name() const;
    virtual bool isConstructor() const;
    std::vector<const wchar_t*> getArgumentNames() const;
    std::vector<DesignScriptClass*> getArgumentTypes(ProtoCore::Core^ core) const;
  
};
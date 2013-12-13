#include "StdAfx.h"

using namespace System;
using namespace System::Collections::Generic;


///////////////////////////////////////////////////
//
// SimpleDSObject implementation
//
///////////////////////////////////////////////////

SimpleDSObject::SimpleDSObject()
    : mpStringData(nullptr)
{
}

SimpleDSObject::SimpleDSObject(System::String^ data)
    : mpStringData(nullptr)
{
    setStringData(data);
}

SimpleDSObject::~SimpleDSObject(void)
{
    delete mpStringData;
}

bool SimpleDSObject::isKindOf(const DesignScriptClass * aClass) const
{
    return false; //not implemented yet
}

DesignScriptClass* SimpleDSObject::isA() const
{
    return nullptr; //not implemented yet
}

int SimpleDSObject::comparedTo(const DesignScriptObject * other) const
{
    if(nullptr == other)
        return -1;
    if(this == other)
        return 0;

    const wchar_t* thisData = this->toString();
    const wchar_t* otherData = other->toString();

    return wcscmp(thisData, otherData); //crude implementation based on string compare
}

bool SimpleDSObject::isEqualTo(const DesignScriptObject * other) const
{
    return comparedTo(other) == 0;
}

const wchar_t* SimpleDSObject::toString() const
{
    if(nullptr == mpStringData)
        const_cast<SimpleDSObject*>(this)->setStringData(getData());

    if(nullptr == mpStringData)
        return nullptr;

    return (*mpStringData);
}

void SimpleDSObject::setStringData(System::String^ data)
{
    mpStringData = new StringToWchar(data);
}

System::String^ SimpleDSObject::getData() const
{
    if(nullptr == mpStringData)
        return nullptr;

    return (*mpStringData);
}

std::vector<DesignScriptClass*>* SimpleDSObject::getClasses() const
{
    return NULL;
}

///////////////////////////////////////////////////
//
// MirrorObjectWrapper implementation
//
///////////////////////////////////////////////////

MirrorObjectWrapper::MirrorObjectWrapper(RuntimeMirror^ mirror)
{
    this->setWrapper(mirror);
}


System::String^ MirrorObjectWrapper::getData() const
{
    return wrapper()->GetStringData();
}

const wchar_t* MirrorObjectWrapper::toString() const
{
    return StringToWchar(getData());    
}

std::vector<DesignScriptClass*>* MirrorObjectWrapper::getClasses() const
{
    return NULL;
}

///////////////////////////////////////////////////
//
// LibraryMirrorWrapper implementation
//
///////////////////////////////////////////////////

LibraryMirrorWrapper::LibraryMirrorWrapper(LibraryMirror^ libMirror)
{
    this->setWrapper(libMirror);
}



std::vector<DesignScriptClass*>* LibraryMirrorWrapper::getClasses() const
{
    List<ClassMirror^>^ classMirrors = wrapper()->GetClasses();
    std::vector<DesignScriptClass*>* classList = new std::vector<DesignScriptClass*>();

    for(int i=0; i < classMirrors->Count; ++i)
    {
        classList->push_back(new ClassMirrorWrapper(classMirrors[i]));
    }
    return classList;
}

const wchar_t* LibraryMirrorWrapper::toString() const
{
    String^ name = wrapper()->LibraryName;
    return StringToWchar(name);
}

///////////////////////////////////////////////////
//
// ClassMirrorWrapper implementation
//
///////////////////////////////////////////////////

ClassMirrorWrapper::ClassMirrorWrapper(ClassMirror^ classMirror)
{
    this->setWrapper(classMirror);
    //mpClassMirror = classMirror;
}

const wchar_t* ClassMirrorWrapper::name() const
{
    String^ name = wrapper()->ClassName;
    return StringToWchar(name);
}

DesignScriptClass* ClassMirrorWrapper::parent() const
{
    return new ClassMirrorWrapper(wrapper()->GetSuperClass());
}

std::vector<DesignScriptMethod*>* ClassMirrorWrapper::getConstructors() const
{
    List<MethodMirror^>^ methodMirrors = wrapper()->GetConstructors();
    std::vector<DesignScriptMethod*>* methodList = new std::vector<DesignScriptMethod*>();

    for(int i=0; i < methodMirrors->Count; ++i)
    {
        methodList->push_back(new MethodMirrorWrapper(methodMirrors[i]));
    }
    return methodList;
}

std::vector<DesignScriptMethod*>* ClassMirrorWrapper::getMethods() const
{
    List<MethodMirror^>^ methodMirrors = wrapper()->GetFunctions();
    std::vector<DesignScriptMethod*>* methodList = new std::vector<DesignScriptMethod*>();

    for(int i=0; i < methodMirrors->Count; ++i)
    {
        methodList->push_back(new MethodMirrorWrapper(methodMirrors[i]));
    }
    return methodList;
}

///////////////////////////////////////////////////
//
// MethodMirrorWrapper implementation
//
///////////////////////////////////////////////////

MethodMirrorWrapper::MethodMirrorWrapper(MethodMirror^ methodMirror)
{
    this->setWrapper(methodMirror);
}

const wchar_t* MethodMirrorWrapper::name() const
{
    String^ name = wrapper()->MethodName;
    return StringToWchar(name);
}

std::vector<const wchar_t*> MethodMirrorWrapper::getArgumentNames() const
{
    List<String^>^ argNames = wrapper()->GetArgumentNames();
    std::vector<const wchar_t*> names;

    for(int i=0; i < argNames->Count; ++i)
    {
        names.push_back(StringToWchar(argNames[i]));
    }
    return names;
}

std::vector<DesignScriptClass*> MethodMirrorWrapper::getArgumentTypes(ProtoCore::Core^ core) const
{
    List<ProtoCore::Type>^ types = wrapper()->GetArgumentTypes();
    std::vector<DesignScriptClass*> classes;

    for(int i = 0; i < types->Count; ++i)
    {
        ClassMirror^ cMirror = gcnew ClassMirror(types[i], core);
        classes.push_back(new ClassMirrorWrapper(cMirror));
    }
    return classes;
}
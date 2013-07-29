#pragma once
#include "WrapperObject.h"

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

    void setStringData(System::String^ data);
protected:
    virtual System::String^ getData() const;

private:
    StringToWchar* mpStringData;
};

class MirrorObjectWrapper : public WrapperObject<RuntimeMirror, SimpleDSObject>
{
public:
    MirrorObjectWrapper(RuntimeMirror^ mirror);
    virtual bool isEqualTo(const DesignScriptObject * other) const;
protected:
    virtual System::String^ getData() const;
};

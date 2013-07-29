#include "StdAfx.h"

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

///////////////////////////////////////////////////
//
// MirrorObjectWrapper implementation
//
///////////////////////////////////////////////////

MirrorObjectWrapper::MirrorObjectWrapper(RuntimeMirror^ mirror)
{
    this->setWrapper(mirror);
}

bool MirrorObjectWrapper::isEqualTo(const DesignScriptObject * other) const
{
    /*const MirrorObjectWrapper* pMirror = dynamic_cast<const MirrorObjectWrapper*>(other);
    if(nullptr == pMirror)
        return __super::isEqualTo(other);

    return wrapper()->Equals(pMirror->wrapper());*/

    return __super::isEqualTo(other);
}

System::String^ MirrorObjectWrapper::getData() const
{
    return wrapper()->GetStringData();
}

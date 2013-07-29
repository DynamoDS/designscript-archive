#include "StdAfx.h"
#include "StringUtils.h"
#include <vcclr.h>

StringToWchar::StringToWchar(System::String^ str)
{
    //pin the string
    m_pinner = (GCHandle::operator System::IntPtr(GCHandle::Alloc(str,System::Runtime::InteropServices::GCHandleType::Pinned))).ToPointer();

    pin_ptr<const wchar_t> tmp = PtrToStringChars(str);
    m_ptr = tmp;
}

StringToWchar::~StringToWchar()
{
    GCHandle g = (GCHandle::operator GCHandle(System::IntPtr(m_pinner)));
    g.Free();
    m_pinner = 0;
}

StringToWchar::operator const wchar_t*() const
{
    return m_ptr;
}

StringToWchar::operator System::String^() const
{
    GCHandle g = (GCHandle::operator GCHandle(System::IntPtr(m_pinner)));
    return static_cast<System::String^>(g.Target);
}


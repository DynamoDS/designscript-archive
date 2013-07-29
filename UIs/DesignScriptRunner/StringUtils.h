#pragma once
class StringToWchar
{
    typedef System::Runtime::InteropServices::GCHandle GCHandle;

    const wchar_t* m_ptr;
    void* m_pinner;

public:

    StringToWchar(System::String^ str);

    virtual ~StringToWchar();

    operator const wchar_t*() const;

    operator System::String^() const;
}; 

inline System::String^ WcharToString(const wchar_t* value)
{
    return gcnew System::String(value);
}

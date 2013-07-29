#pragma once

#include <gcroot.h>
#include "DesignScriptRunnerAPI.h"


template <typename RCW, typename Base>
class WrapperObject : public Base
{
    gcroot<RCW^> m_pWrapper;
public:
    WrapperObject() 
    {
    }
    virtual ~WrapperObject()
    {
        
    }
    RCW^ wrapper() const { return m_pWrapper;}
    void setWrapper(RCW^ pRCW)
    {
        m_pWrapper = pRCW;
    }
};



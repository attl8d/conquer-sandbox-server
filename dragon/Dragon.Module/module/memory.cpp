#include "memory.h"

namespace dragon {

    void memory::write(LPVOID IpBaseAddress, LPCVOID IpBuffer, SIZE_T nSize)
    {
        DWORD Protect = NULL;
        VirtualProtect(IpBaseAddress, nSize, PAGE_EXECUTE_READWRITE, &Protect);
        WriteProcessMemory(GetCurrentProcess(), IpBaseAddress, IpBuffer, nSize, NULL);
        VirtualProtect(IpBaseAddress, nSize, Protect, &Protect);
    }

    void memory::write(LPVOID IpBaseAddress, LPCVOID IpBuffer, SIZE_T nSize, SIZE_T* lpNumberOfBytesWritten)
    {
        WriteProcessMemory(GetCurrentProcess(), IpBaseAddress, IpBuffer, nSize, lpNumberOfBytesWritten);
    }

    void memory::read(LPCVOID IpBaseAddress, LPVOID IpBuffer, SIZE_T nSize, SIZE_T* lpNumberOfBytesWritten)
    {
        ReadProcessMemory(GetCurrentProcess(), IpBaseAddress, IpBuffer, nSize, lpNumberOfBytesWritten);
    }


}
#pragma once

#include <wtypes.h>

namespace dragon {

	class memory
	{
	public:
		static void write(LPVOID IpBaseAddress, LPCVOID IpBuffer, SIZE_T nSize);
		static void write(LPVOID IpBaseAddress, LPCVOID IpBuffer, SIZE_T nSize, SIZE_T* lpNumberOfBytesWritten);
		static void read(LPCVOID IpBaseAddress, LPVOID IpBuffer, SIZE_T nSize, SIZE_T* lpNumberOfBytesWritten);
	};
}

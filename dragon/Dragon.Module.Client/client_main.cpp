#include "client.h"
#include <memory>
#include "client_hook.h"

using namespace std;

// The entry point into the module. When the library gets attached using Dragon, it calls this
// function using an existing or new thread. The program also calls this function when unloading
// the module.
bool APIENTRY DllMain(HMODULE hModule, DWORD dwReason, LPVOID lpReserved)
{
    if (dwReason == DLL_PROCESS_ATTACH) {        
        dragon::ClientHook::init();
    }
    else if (dwReason == DLL_PROCESS_DETACH) {
        
    }

    return true;
}

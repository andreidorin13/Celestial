#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <WinSock2.h>
#include <WS2tcpip.h>
#include <cstdio>
#include <vector>

#pragma pack(push, 1)
struct vec3 { float x, y, z; };
#pragma pack(pop)

int FilterEx(unsigned int code, struct _EXCEPTION_POINTERS* ep) {
    if (code == EXCEPTION_ACCESS_VIOLATION) {
#ifdef _DEBUG
        printf("Got bad pointer\n");
#endif // DEBUG
        return EXCEPTION_EXECUTE_HANDLER;
    }        
    else {
#ifdef _DEBUG
        printf("Caught something else\n");
#endif // DEBUG
        return EXCEPTION_CONTINUE_SEARCH;
    }
}

uintptr_t FollowPtr(uintptr_t ptr, const std::vector<unsigned int>& offsets) {
    auto addr = ptr;

    __try {
        for (const auto& i : offsets) {
            addr = *(uintptr_t*)addr;
            addr += i;
        }
    }
    __except (FilterEx(GetExceptionCode(), GetExceptionInformation())) {
        return -1;
    }

    return addr;
}

FILE* CreateConsole() {
    AllocConsole();
    FILE* f = new FILE();
    freopen_s(&f, "CONOUT$", "w", stdout);
    return f;
}

SOCKET CreateSocket() {
    WSADATA wsaData;
    WSAStartup(MAKEWORD(2, 2), &wsaData);

    struct addrinfo* result = nullptr;
    struct addrinfo hints {};
    hints.ai_family = AF_INET;
    hints.ai_socktype = SOCK_STREAM;
    hints.ai_protocol = IPPROTO_TCP;
    hints.ai_flags = AI_PASSIVE;

    getaddrinfo(NULL, "8000", &hints, &result);
    auto sock = socket(result->ai_family, result->ai_socktype, result->ai_protocol);
    bind(sock, result->ai_addr, (int)result->ai_addrlen);
    listen(sock, SOMAXCONN);
    freeaddrinfo(result);

#ifdef _DEBUG
    printf("Created Socket\n");
#endif // _DEBUG
    return sock;
}

DWORD WINAPI Astro(HMODULE hModule) {
#ifdef _DEBUG
    auto f = CreateConsole();
#endif // _DEBUG
    auto sock = CreateSocket();

#ifdef _DEBUG
    printf("Astro Loaded!\n");
#endif // _DEBUG

	auto base = (uintptr_t)GetModuleHandle(L"SoTGame.exe");
    const std::vector<unsigned int> offsets = { 0x110, 0x340, 0x3A0, 0x2C8, 0x660, 0xD0, 0x150 };

START:
#ifdef _DEBUG
    printf("Awaiting Client\n");
#endif // DEBUG

    auto client = accept(sock, NULL, NULL);
    vec3* vec = nullptr;
    int res;
    while (true) {
		auto pos = FollowPtr(base + 0x07262EA8, offsets);

		if (pos == -1)
            goto END;

		vec = (vec3*)pos;
#ifdef _DEBUG
        printf("x: %f y: %f z: %f\n", vec->x, vec->y, vec->z);
#endif // _DEBUG
        res = send(client, (char*)vec, sizeof(vec3), 0);
        if (res == SOCKET_ERROR) {
#ifdef _DEBUG
            printf("Client disconnected\n");
#endif // _DEBUG
            goto START;
        }
	END:
        Sleep(500);
    }

#ifdef _DEBUG
    fclose(f);
    delete f;
    FreeConsole();
#endif // _DEBUG
    FreeLibraryAndExitThread(hModule, 0);
    return 0;
}

BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved) {
    switch (ul_reason_for_call) {
    case DLL_PROCESS_ATTACH: {
        CloseHandle(CreateThread(nullptr, 0, (LPTHREAD_START_ROUTINE)Astro, hModule, 0, nullptr));
        break;
    }
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}
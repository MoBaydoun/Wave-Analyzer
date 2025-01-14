#ifdef __cplusplus
#define EXPORT extern "C" __declspec (dllexport)
#else
#define EXPORT __declspec (dllexport)
#endif

EXPORT INT start();
EXPORT PBYTE getBuffer();
EXPORT VOID setBuffer(PBYTE, DWORD, int, int, int, int, int);
EXPORT DWORD getDwLength();
EXPORT VOID setDwLength(int);
EXPORT LRESULT StartMessage();
EXPORT LRESULT StopMessage();
EXPORT LRESULT PlayMessage();
EXPORT LRESULT PlayStopMessage();
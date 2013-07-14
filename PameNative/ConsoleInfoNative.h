//////////////////////////////////////////////////////////////////////////////////////
//
// PAME Copyright (c) George Samartzidis <samartzidis@gmail.com>. All rights reserved.
// You are not allowed to redistribute, modify or sell any part of this file in either 
// compiled or non-compiled form without the author's written permission.
//
//////////////////////////////////////////////////////////////////////////////////////
//

// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the CONSOLEINFONATIVE_EXPORTS
// symbol defined on the command line. this symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// CONSOLEINFONATIVE_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
//
#ifdef PAMENATIVE_EXPORTS
	#define PAMENATIVE_API __declspec(dllexport)
#else
	#define PAMENATIVE_API __declspec(dllimport)
#endif

#ifdef __cplusplus
extern "C" 
{
#endif

#if !defined(_WINCON_)
	typedef struct CONSOLEINFONATIVE_API _CONSOLE_FONT_INFOEX
	{
		ULONG cbSize;
		DWORD nFont;
		COORD dwFontSize;
		UINT FontFamily;
		UINT FontWeight;
		WCHAR FaceName[LF_FACESIZE];
	} CONSOLE_FONT_INFOEX, *PCONSOLE_FONT_INFOEX;
#endif

#pragma pack(push, 1)
//
//	Structure to send console via WM_SETCONSOLEINFO
//
typedef struct PAMENATIVE_API _CONSOLE_INFO
{
	ULONG		Length;
	COORD		ScreenBufferSize;
	COORD		WindowSize;
	ULONG		WindowPosX;
	ULONG		WindowPosY;

	COORD		FontSize;
	ULONG		FontFamily;
	ULONG		FontWeight;
	WCHAR		FaceName[32];

	ULONG		CursorSize;
	ULONG		FullScreen;
	ULONG		QuickEdit;
	ULONG		AutoPosition;
	ULONG		InsertMode;
	
	USHORT		ScreenColors;
	USHORT		PopupColors;
	ULONG		HistoryNoDup;
	ULONG		HistoryBufferSize;
	ULONG		NumberOfHistoryBuffers;
	
	COLORREF	ColorTable[16];

	ULONG		CodePage;
	HWND		Hwnd;

	WCHAR		ConsoleTitle[0x100];

} CONSOLE_INFO;
#pragma pack(pop)

void PAMENATIVE_API GetConsoleSizeInfo(CONSOLE_INFO *pci);
BOOL PAMENATIVE_API SetConsoleInfo(HWND hwndConsole, CONSOLE_INFO *pci);
void PAMENATIVE_API SetConsoleFontSizeTo(HWND inConWnd, int inSizeY, int inSizeX, wchar_t *asFontName);

#ifdef __cplusplus
}// extern "C" 
#endif
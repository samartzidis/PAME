//////////////////////////////////////////////////////////////////////////////////////
//
// PAME Copyright (c) George Samartzidis <samartzidis@gmail.com>. All rights reserved.
// You are not allowed to redistribute, modify or sell any part of this file in either 
// compiled or non-compiled form without the author's written permission.
//
// This file contains code portions from the conemu-maximus5 project, 
// Copyright (c) 2009-2010 Maximus5.
//
//////////////////////////////////////////////////////////////////////////////////////
//

#include "stdafx.h"
#include "ConsoleInfoNative.h"

#include <stdarg.h>
#include <tchar.h>
#include <strsafe.h>

//////////////////////////////////////////////////////////////////////////////////////
// Only in Win2k+  (use FindWindow for NT4)
// HWND WINAPI GetConsoleWindow();

// Undocumented console message
#define WM_SETCONSOLEINFO (WM_USER+201)

typedef BOOL (WINAPI *PGetCurrentConsoleFontEx)
	(__in HANDLE hConsoleOutput,__in BOOL bMaximumWindow,__out PCONSOLE_FONT_INFOEX lpConsoleCurrentFontEx);
typedef BOOL (WINAPI *PSetCurrentConsoleFontEx)
	(__in HANDLE hConsoleOutput,__in BOOL bMaximumWindow,__out PCONSOLE_FONT_INFOEX lpConsoleCurrentFontEx);

////////////////////////////////////////////////////////////////////////////////////
//
void DisplayOSErrorMessage(DWORD dwLastError, LPCTSTR lpszCustomMessage)
{
	LPVOID lpMsgBuf;
	LPVOID lpDisplayBuf;

	FormatMessage(
		FORMAT_MESSAGE_ALLOCATE_BUFFER | 
		FORMAT_MESSAGE_FROM_SYSTEM |
		FORMAT_MESSAGE_IGNORE_INSERTS,
		NULL,
		dwLastError,
		MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
		(LPTSTR) &lpMsgBuf,
		0, NULL );

	lpDisplayBuf = (LPVOID)LocalAlloc(LMEM_ZEROINIT, 
        (lstrlen((LPCTSTR)lpMsgBuf) + lstrlen((LPCTSTR)lpszCustomMessage) + 40) * sizeof(TCHAR));

	StringCchPrintf((LPTSTR)lpDisplayBuf, LocalSize(lpDisplayBuf) / sizeof(TCHAR), 
		_T("%s. %s (ErrCode = %i)"), lpszCustomMessage, lpMsgBuf, dwLastError);
	MessageBox(NULL, (LPCTSTR)lpDisplayBuf, _T("PameNative"), MB_OK | MB_ICONSTOP | MB_SETFOREGROUND);
	
	LocalFree(lpMsgBuf);
	LocalFree(lpDisplayBuf);
}

//////////////////////////////////////////////////////////////////////////////////////
// Wrapper around WM_SETCONSOLEINFO. We need to create the
// necessary section (file-mapping) object in the context of the
// process which owns the console, before posting the message
//
BOOL SetConsoleInfo(HWND hwndConsole, CONSOLE_INFO *pci)
{
	DWORD   dwConsoleOwnerPid, dwCurProcId, dwConsoleThreadId;
	HANDLE  hProcess = NULL;
	HANDLE	hSection = NULL, hDupSection=NULL;
	PVOID   ptrView = 0;
	DWORD   dwLastError = 0;
	
	//
	//	Open the process which "owns" the console
	//	
	dwCurProcId = GetCurrentProcessId();
	dwConsoleThreadId = GetWindowThreadProcessId(hwndConsole, &dwConsoleOwnerPid);
	hProcess = GetCurrentProcess();

	
	GetWindowThreadProcessId(hwndConsole, &dwConsoleOwnerPid);	
	hProcess = OpenProcess(MAXIMUM_ALLOWED, FALSE, dwConsoleOwnerPid);
	dwLastError = GetLastError();

	if (hProcess==NULL) 
	{
		if (dwConsoleOwnerPid == dwCurProcId) 
			hProcess = GetCurrentProcess();
		else 
		{
			DisplayOSErrorMessage(dwLastError, _T("Can't open console process"));
			return FALSE;
		}
	}
	
	//
	// Create a SECTION object backed by page-file, then map a view of
	// this section into the owner process so we can write the contents 
	// of the CONSOLE_INFO buffer into it
	//
	hSection = CreateFileMapping(INVALID_HANDLE_VALUE, 0, PAGE_READWRITE, 0, pci->Length, 0);
	if (!hSection) 
	{
		dwLastError = GetLastError();
		DisplayOSErrorMessage(dwLastError, _T("CreateFileMapping failed"));
	} 
	else 
	{
		//
		//	Copy our console structure into the section-object
		//
		ptrView = MapViewOfFile(hSection, FILE_MAP_WRITE|FILE_MAP_READ, 0, 0, pci->Length);
		if (!ptrView) 
		{
			dwLastError = GetLastError();
			DisplayOSErrorMessage(dwLastError, _T("MapViewOfFile failed"));
		} 
		else 
		{
			memcpy(ptrView, pci, pci->Length);

			UnmapViewOfFile(ptrView);

			//
			//	Map the memory into owner process
			//
			if (!DuplicateHandle(GetCurrentProcess(), hSection, 
				hProcess, &hDupSection, 0, FALSE, DUPLICATE_SAME_ACCESS) || !hDupSection)
			{
				dwLastError = GetLastError();
				DisplayOSErrorMessage(dwLastError, _T("DuplicateHandle failed"));
			} 
			else 
			{
				//  Send console window the "update" message
				LRESULT dwConInfoRc = 0;
				DWORD dwConInfoErr = 0;
				
				dwConInfoRc = SendMessage(hwndConsole, WM_SETCONSOLEINFO, (WPARAM)hDupSection, 0);
				dwConInfoErr = GetLastError();
			}
		}
	}

	//
	// clean up
	//
	if (hDupSection) 
	{
		if (dwConsoleOwnerPid == dwCurProcId) 
			CloseHandle(hDupSection);
		else 
		{
			HANDLE hThread = NULL;
			hThread = CreateRemoteThread(hProcess, 0, 0, 
				(LPTHREAD_START_ROUTINE)CloseHandle, hDupSection, 0, 0);
			if (hThread) 
				CloseHandle(hThread);
		}
	}

	if (hSection)
		CloseHandle(hSection);
	if ((dwConsoleOwnerPid!=dwCurProcId) && hProcess)
		CloseHandle(hProcess);

	return TRUE;
}

//////////////////////////////////////////////////////////////////////////////////////
//	Fill the CONSOLE_INFO structure with information
//  about the current console window
//
void GetConsoleSizeInfo(CONSOLE_INFO *pci)
{
	CONSOLE_SCREEN_BUFFER_INFO csbi;
	HANDLE hConsoleOut = GetStdHandle(STD_OUTPUT_HANDLE);
	GetConsoleScreenBufferInfo(hConsoleOut, &csbi);

	pci->ScreenBufferSize = csbi.dwSize;
	pci->WindowSize.X = csbi.srWindow.Right - csbi.srWindow.Left + 1;
	pci->WindowSize.Y = csbi.srWindow.Bottom - csbi.srWindow.Top + 1;
	pci->WindowPosX = csbi.srWindow.Left;
	pci->WindowPosY = csbi.srWindow.Top;
}

//////////////////////////////////////////////////////////////////////////////////////
// Set palette of current console
//
//	palette should be of the form:
//
// COLORREF DefaultColors[16] = 
// {
//	0x00000000, 0x00800000, 0x00008000, 0x00808000,
//	0x00000080, 0x00800080, 0x00008080, 0x00c0c0c0, 
//	0x00808080,	0x00ff0000, 0x0000ff00, 0x00ffff00,
//	0x000000ff, 0x00ff00ff,	0x0000ffff, 0x00ffffff
// };
//
VOID WINAPI SetConsolePalette(COLORREF palette[16])
{
	CONSOLE_INFO ci = { sizeof(ci) };
	int i;
        HWND hwndConsole = GetConsoleWindow();

	// get current size/position settings rather than using defaults..
	GetConsoleSizeInfo(&ci);

	// set these to zero to keep current settings
	ci.FontSize.X = 0;//8;
	ci.FontSize.Y = 0;//12;
	ci.FontFamily = 0;//0x30;//FF_MODERN|FIXED_PITCH;//0x30;
	ci.FontWeight = 0;//0x400;
	//lstrcpyW(ci.FaceName, L"Terminal");
	ci.FaceName[0] = L'\0';

	ci.CursorSize = 25;
	ci.FullScreen = FALSE;
	ci.QuickEdit = TRUE;
	ci.AutoPosition = 0x10000;
	ci.InsertMode = TRUE;
	ci.ScreenColors = MAKEWORD(0x7, 0x0);
	ci.PopupColors = MAKEWORD(0x5, 0xf);
	ci.HistoryNoDup = FALSE;
	ci.HistoryBufferSize = 50;
	ci.NumberOfHistoryBuffers = 4;

	// colour table
	for(i = 0; i < 16; i++)
		ci.ColorTable[i] = palette[i];

	ci.CodePage = GetConsoleOutputCP();
	ci.Hwnd = hwndConsole;
	lstrcpyW(ci.ConsoleTitle, L"");
	SetConsoleInfo(hwndConsole, &ci);
}

//////////////////////////////////////////////////////////////////////////////////////
//
void SetConsoleFontSizeTo(HWND inConWnd, int inSizeY, int inSizeX, wchar_t *asFontName)
{
	HMODULE hKernel = GetModuleHandle(L"kernel32.dll");
	if (!hKernel)
		return;
	PGetCurrentConsoleFontEx GetCurrentConsoleFontEx = (PGetCurrentConsoleFontEx)
		GetProcAddress(hKernel, "GetCurrentConsoleFontEx");
	PSetCurrentConsoleFontEx SetCurrentConsoleFontEx = (PSetCurrentConsoleFontEx)
		GetProcAddress(hKernel, "SetCurrentConsoleFontEx");

	if (GetCurrentConsoleFontEx && SetCurrentConsoleFontEx) // We have Vista
	{
		CONSOLE_FONT_INFOEX cfi = {sizeof(CONSOLE_FONT_INFOEX)};

		//GetCurrentConsoleFontEx(GetStdHandle(STD_OUTPUT_HANDLE), FALSE, &cfi);
		cfi.dwFontSize.X = inSizeX;
		cfi.dwFontSize.Y = inSizeY;

		//TODO:
		lstrcpynW(cfi.FaceName, asFontName ? asFontName : L"Lucida Console", LF_FACESIZE);
		SetCurrentConsoleFontEx(GetStdHandle(STD_OUTPUT_HANDLE), FALSE, &cfi);
	}
	else // We have other NT
	{
		const COLORREF DefaultColors[16] = 
		{
			0x00000000, 0x00800000, 0x00008000, 0x00808000,
			0x00000080, 0x00800080, 0x00008080, 0x00c0c0c0, 
			0x00808080,	0x00ff0000, 0x0000ff00, 0x00ffff00,
			0x000000ff, 0x00ff00ff,	0x0000ffff, 0x00ffffff
		};

		CONSOLE_INFO ci = { sizeof(ci) };
		int i;

		// get current size/position settings rather than using defaults..
		GetConsoleSizeInfo(&ci);

		// set these to zero to keep current settings
		ci.FontSize.X = inSizeX;
		ci.FontSize.Y = inSizeY;
		ci.FontFamily = 0;//0x30;//FF_MODERN|FIXED_PITCH;//0x30;
		ci.FontWeight = 0;//0x400;
		lstrcpynW(ci.FaceName, asFontName ? asFontName : L"Lucida Console", 32);
		ci.CursorSize = 25;
		ci.FullScreen = FALSE;
		ci.QuickEdit = FALSE;
		ci.AutoPosition = 0x10000;
		ci.InsertMode = TRUE;
		ci.ScreenColors = MAKEWORD(0x7, 0x0);
		ci.PopupColors = MAKEWORD(0x5, 0xf);
		ci.HistoryNoDup = FALSE;
		ci.HistoryBufferSize = 50;
		ci.NumberOfHistoryBuffers = 4;

		// color table
		for(i = 0; i < 16; i++)
			ci.ColorTable[i] = DefaultColors[i];

		ci.CodePage = GetConsoleOutputCP();
		ci.Hwnd = inConWnd;

		*ci.ConsoleTitle = NULL;

		SetConsoleInfo(inConWnd, &ci);
	}
}
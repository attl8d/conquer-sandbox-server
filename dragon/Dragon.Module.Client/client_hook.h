#pragma once
#include <string>

namespace dragon {

	class ClientHook
	{
	public:
		static void init();
		static void serverfile();
		static void resolution();
		static void change_gui(int width, int hight);
		static void change_fps();
	};

}
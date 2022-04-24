#include "client.h"
#include "client_hook.h"
#include "module/memory.h"
#include <stdlib.h>
#include <stdio.h>

namespace dragon {

    const LPVOID addr_server_dat1 = (LPVOID)0x0067CF30;
    const LPVOID addr_server_dat2 = (LPVOID)0x005B0BD2;
    const LPVOID addr_server_dat3 = (LPVOID)0x005B0C9A;
    const LPVOID addr_server_dat4 = (LPVOID)0x0067CF70;

    const LPVOID addr_width1 = (LPVOID)(0x004EB22F + 6);
    const LPVOID addr_hight1 = (LPVOID)(0x004EB239 + 6);
    const LPVOID addr_width2 = (LPVOID)(0x004ED815 + 6);
    const LPVOID addr_hight2 = (LPVOID)(0x004ED81F + 6);

    const LPVOID addr_puzzle_width = (LPVOID)(0x00539C34 + 1);
    const LPVOID addr_puzzle_hight = (LPVOID)(0x00539C4B + 1);
    const LPVOID addr_puzzle_hight2 = (LPVOID)(0x00539C6B + 1);

    const LPVOID addr_main_ui_buf = (LPVOID)(0x004F7B8A + 0);
    const LPVOID addr_main_ui_y = (LPVOID)(0x004F7B8A + 1);
    const LPVOID addr_main_ui_x = (LPVOID)(0x004F7B8F + 1);

    const LPVOID addr_help_button = (LPVOID)(0x0045649C + 1);

    const LPVOID addr_arrow_icon = (LPVOID)(0x00553B2D + 3);

    const LPVOID addr_fps1 = (LPVOID)(0x004EB3AB + 2);
    const LPVOID addr_fps2 = (LPVOID)(0x004EB3B4 + 2);

    const LPVOID addr_default_seed = (LPVOID)0x00698160;

    void dragon::ClientHook::init()
    {
        dragon::ClientHook::serverfile();
        dragon::ClientHook::resolution();
        dragon::ClientHook::change_fps();
    }

    void dragon::ClientHook::serverfile()
    {
        const char server_dat[] = ".\\Server.dat"; // any name

        BYTE server_dat_buf1[] = { 0xE9, 0x99, 0xC3, 0x0C, 0x00, 0x90, 0x90, 0x90 };
        BYTE server_dat_buf2[] = { 0xB3, 0x01 };
        BYTE server_dat_buf3[] = { 0x68, 0x30, 0xCF, 0x67, 0x00, 0xE8, 0x06, 0xF5, 0xFB, 0xFF, 0xE9, 0x5B, 0x3C, 0xF3, 0xFF };

        memory::write(addr_server_dat1, &server_dat, sizeof server_dat, NULL);
        memory::write(addr_server_dat2, &server_dat_buf1, sizeof server_dat_buf1, NULL);
        memory::write(addr_server_dat3, &server_dat_buf2, sizeof server_dat_buf2, NULL);
        memory::write(addr_server_dat4, &server_dat_buf3, sizeof server_dat_buf3, NULL);

        const char default_seed[] = "DR654dt34trg4UI6";
        memory::write(addr_default_seed, &default_seed, sizeof default_seed);
    }

    void dragon::ClientHook::resolution()
    {
        const char GAME_SETUP_INI[] = ".\\ini\\GameSetUp.ini";

        int width = GetPrivateProfileIntA("ScreenMode", "ScrWidth", 1024, GAME_SETUP_INI);
        int hight = GetPrivateProfileIntA("ScreenMode", "ScrHeight", 768, GAME_SETUP_INI);

        memory::write(addr_width1, &width, 4, NULL);
        memory::write(addr_hight1, &hight, 4, NULL);
        memory::write(addr_width2, &width, 4, NULL);
        memory::write(addr_hight2, &hight, 4, NULL);

        memory::write(addr_puzzle_width, &width, 4, NULL);
        memory::write(addr_puzzle_hight, &hight, 4, NULL);
        memory::write(addr_puzzle_hight2, &hight, 4, NULL);

        BYTE main_ui_buf[] = { 0x68, 0xFF, 0x00, 0x00, 0x00, 0x68, 0xFF, 0x00, 0x00,
                               0x00, 0x8D, 0x8E, 0xE0, 0x04, 0x00, 0x00, 0x90, 0x90, 0x90 };
        memory::write(addr_main_ui_buf, &main_ui_buf, sizeof main_ui_buf, NULL);

        int main_ui_y = (hight - 141);
        int main_ui_x = (width - 1024) / 2;
        memory::write(addr_main_ui_y, &main_ui_y, 4, NULL);
        memory::write(addr_main_ui_x, &main_ui_x, 4, NULL);

        int position = -30;
        memory::write(addr_help_button, &position, 1, NULL);

        position = main_ui_x + 131;
        memory::write(addr_arrow_icon, &position, 4, NULL);

        dragon::ClientHook::change_gui(width, hight);
    }

    void dragon::ClientHook::change_gui(int width, int hight)
    {
        int main_ui_y = (hight - 141);
        int main_ui_x = (width - 1024) / 2;

        const char GUI_SETUP_INI[] = ".\\ini\\GUI.ini";
        char buf[10];

        // Main interface
        sprintf_s(buf, "%d", main_ui_x);
        WritePrivateProfileStringA("0-130", "x", buf, GUI_SETUP_INI);
        sprintf_s(buf, "%d", main_ui_y);
        WritePrivateProfileStringA("0-130", "y", buf, GUI_SETUP_INI);

        // Chat panel
        sprintf_s(buf, "%d", main_ui_x + 82);
        WritePrivateProfileStringA("0-145", "x", buf, GUI_SETUP_INI);
        sprintf_s(buf, "%d", hight - 71);
        WritePrivateProfileStringA("0-145", "y", buf, GUI_SETUP_INI);

        // Chat Player Names
        sprintf_s(buf, "%d", main_ui_x + 82 + 35);
        WritePrivateProfileStringA("0-148", "x", buf, GUI_SETUP_INI);
        sprintf_s(buf, "%d", hight - 365);
        WritePrivateProfileStringA("0-148", "y", buf, GUI_SETUP_INI);

        // Chat Channel
        sprintf_s(buf, "%d", main_ui_x + 82 + 172);
        WritePrivateProfileStringA("0-174", "x", buf, GUI_SETUP_INI);
        sprintf_s(buf, "%d", hight - 325);
        WritePrivateProfileStringA("0-174", "y", buf, GUI_SETUP_INI);

        // Path Finding Button
        sprintf_s(buf, "%d", width - 110);
        WritePrivateProfileStringA("0-304", "x", buf, GUI_SETUP_INI);

        // Path Finding GUI
        sprintf_s(buf, "%d", width - 530);
        WritePrivateProfileStringA("0-303", "x", buf, GUI_SETUP_INI);

        // Exit Game
        sprintf_s(buf, "%d", (width - 288) / 2);
        WritePrivateProfileStringA("0-158", "x", buf, GUI_SETUP_INI);
        sprintf_s(buf, "%d", (hight - 120) / 2);
        WritePrivateProfileStringA("0-158", "y", buf, GUI_SETUP_INI);

        // Options
        sprintf_s(buf, "%d", (width - 370) / 2);
        WritePrivateProfileStringA("0-138", "x", buf, GUI_SETUP_INI);
        sprintf_s(buf, "%d", (hight - 350) / 2);
        WritePrivateProfileStringA("0-138", "y", buf, GUI_SETUP_INI);

        // Team
        sprintf_s(buf, "%d", (width - 290) / 2);
        WritePrivateProfileStringA("0-141", "x", buf, GUI_SETUP_INI);
        sprintf_s(buf, "%d", (width - 58) / 2);
        WritePrivateProfileStringA("0-268", "x", buf, GUI_SETUP_INI);

        // VIP Button
        sprintf_s(buf, "%d", main_ui_x + 82 + 202);
        WritePrivateProfileStringA("0-339", "x", buf, GUI_SETUP_INI);
        sprintf_s(buf, "%d", hight - 115);
        WritePrivateProfileStringA("0-339", "y", buf, GUI_SETUP_INI);

        // My Talisman UI
        //sprintf_s(buf, "%d", width + 000);
        //WritePrivateProfileStringA("0-345", "x", buf, IpFile);

        // Target Talisman UI
        sprintf_s(buf, "%d", width - 345);
        WritePrivateProfileStringA("0-346", "x", buf, GUI_SETUP_INI);

        // Shopping Mall
        sprintf_s(buf, "%d", main_ui_x + 82 + 50);
        WritePrivateProfileStringA("0-289", "x", buf, GUI_SETUP_INI);
        sprintf_s(buf, "%d", hight - 115);
        WritePrivateProfileStringA("0-289", "y", buf, GUI_SETUP_INI);

        // Mentor
        sprintf_s(buf, "%d", main_ui_x + 82 + 85);
        WritePrivateProfileStringA("0-325", "x", buf, GUI_SETUP_INI);
        sprintf_s(buf, "%d", hight - 130);
        WritePrivateProfileStringA("0-325", "y", buf, GUI_SETUP_INI);

        // Item Lock
        sprintf_s(buf, "%d", main_ui_x + 82 + 145);
        WritePrivateProfileStringA("0-328", "x", buf, GUI_SETUP_INI);
        sprintf_s(buf, "%d", hight - 110);
        WritePrivateProfileStringA("0-328", "y", buf, GUI_SETUP_INI);

        // Whisper Player Avatars
        sprintf_s(buf, "%d", main_ui_x + 82 + 530);
        WritePrivateProfileStringA("0-3", "x", buf, GUI_SETUP_INI);
        sprintf_s(buf, "%d", hight - 115);
        WritePrivateProfileStringA("0-3", "y", buf, GUI_SETUP_INI);

        // PKModes
        sprintf_s(buf, "%d", main_ui_x + 82 + 500);
        WritePrivateProfileStringA("0-191", "x", buf, GUI_SETUP_INI);
        sprintf_s(buf, "%d", hight - 100);
        WritePrivateProfileStringA("0-191", "y", buf, GUI_SETUP_INI);

        // Actions
        //sprintf_s(buf, "%d", main_ui_x + 250);
        //WritePrivateProfileStringA("0-140", "x", buf, IpFile);
        //sprintf_s(buf, "%d", hight - 150);
        //WritePrivateProfileStringA("0-140", "y", buf, IpFile);

        // Map Mini
        sprintf_s(buf, "%d", width - 40);
        WritePrivateProfileStringA("0-1199", "x", buf, GUI_SETUP_INI);

        // Map Full
        sprintf_s(buf, "%d", width - 20);
        WritePrivateProfileStringA("0-1200", "x", buf, GUI_SETUP_INI);

        // Arena
        sprintf_s(buf, "%d", main_ui_x + 82 + 250);
        WritePrivateProfileStringA("0-403", "x", buf, GUI_SETUP_INI);
        sprintf_s(buf, "%d", hight - 115);
        WritePrivateProfileStringA("0-403", "y", buf, GUI_SETUP_INI);

        // Action Express Interact Combobox
        sprintf_s(buf, "%d", main_ui_x + 82 + 350);
        WritePrivateProfileStringA("0-367", "x", buf, GUI_SETUP_INI);
        sprintf_s(buf, "%d", hight - 245);
        WritePrivateProfileStringA("0-367", "y", buf, GUI_SETUP_INI);
        // Action
        sprintf_s(buf, "%d", main_ui_x + 82 + 350);
        WritePrivateProfileStringA("0-140", "x", buf, GUI_SETUP_INI);
        sprintf_s(buf, "%d", hight - 225);
        WritePrivateProfileStringA("0-140", "y", buf, GUI_SETUP_INI);
        // Express
        sprintf_s(buf, "%d", main_ui_x + 82 + 350);
        WritePrivateProfileStringA("0-274", "x", buf, GUI_SETUP_INI);
        sprintf_s(buf, "%d", hight - 215);
        WritePrivateProfileStringA("0-274", "y", buf, GUI_SETUP_INI);
        // Interact
        sprintf_s(buf, "%d", main_ui_x + 82 + 350);
        WritePrivateProfileStringA("0-360", "x", buf, GUI_SETUP_INI);
        sprintf_s(buf, "%d", hight - 215);
        WritePrivateProfileStringA("0-360", "y", buf, GUI_SETUP_INI);

        // Group UI
        sprintf_s(buf, "%d", main_ui_x + 82 + 600);
        WritePrivateProfileStringA("0-371", "x", buf, GUI_SETUP_INI);
        sprintf_s(buf, "%d", hight - 140);
        WritePrivateProfileStringA("0-371", "y", buf, GUI_SETUP_INI);

        // Message Channel
        sprintf_s(buf, "%d", hight - 453);
        WritePrivateProfileStringA("0-357", "y", buf, GUI_SETUP_INI);

        // Message Scrollbar
        sprintf_s(buf, "%d", hight - 477);
        WritePrivateProfileStringA("0-1198", "y", buf, GUI_SETUP_INI);

        // Mount Vigor
        sprintf_s(buf, "%d", width - 200);
        WritePrivateProfileStringA("0-383", "x", buf, GUI_SETUP_INI);
        sprintf_s(buf, "%d", hight - 200);
        WritePrivateProfileStringA("0-383", "y", buf, GUI_SETUP_INI);

        // Arena qualifier main ui
        sprintf_s(buf, "%d", (width - 665) / 2);
        WritePrivateProfileStringA("0-402", "x", buf, GUI_SETUP_INI);
        sprintf_s(buf, "%d", (hight - 560) / 2);
        WritePrivateProfileStringA("0-402", "y", buf, GUI_SETUP_INI);
        // Arena qualifier opponent info
        sprintf_s(buf, "%d", (width - 290) / 2);
        WritePrivateProfileStringA("0-409", "x", buf, GUI_SETUP_INI);
        sprintf_s(buf, "%d", (hight - 120) / 2);
        WritePrivateProfileStringA("0-409", "y", buf, GUI_SETUP_INI);
        // Arena qualifier fighting points info
        sprintf_s(buf, "%d", (width - 310) / 2);
        WritePrivateProfileStringA("0-411", "x", buf, GUI_SETUP_INI);
        // Arena qualifier quit button
        sprintf_s(buf, "%d", main_ui_x + 82 + 320);
        WritePrivateProfileStringA("0-412", "x", buf, GUI_SETUP_INI);
        sprintf_s(buf, "%d", hight - 130);
        WritePrivateProfileStringA("0-412", "y", buf, GUI_SETUP_INI);
        // Arena qualifier count down box
        sprintf_s(buf, "%d", (width - 290) / 2);
        WritePrivateProfileStringA("0-407", "x", buf, GUI_SETUP_INI);
        sprintf_s(buf, "%d", (hight - 110) / 2);
        WritePrivateProfileStringA("0-407", "y", buf, GUI_SETUP_INI);
        // Arena qualifier result box 
        sprintf_s(buf, "%d", (width - 300) / 2);
        WritePrivateProfileStringA("0-408", "x", buf, GUI_SETUP_INI);
        sprintf_s(buf, "%d", (hight - 150) / 2);
        WritePrivateProfileStringA("0-408", "y", buf, GUI_SETUP_INI);

        const char INFO_SETUP_INI[] = ".\\ini\\info.ini";

        // Exp
        sprintf_s(buf, "%d", (width / 2) - 100);
        WritePrivateProfileStringA("ExpShowPos", "Exp_XPos", buf, INFO_SETUP_INI);
        sprintf_s(buf, "%d", hight - 92);
        WritePrivateProfileStringA("ExpShowPos", "Exp_YPos", buf, INFO_SETUP_INI);

        // AddExp
        sprintf_s(buf, "%d", (width / 2) - 100 + 90);
        WritePrivateProfileStringA("ExpShowPos", "AddExp_XPos", buf, INFO_SETUP_INI);
        sprintf_s(buf, "%d", hight - 92);
        WritePrivateProfileStringA("ExpShowPos", "AddExp_YPos", buf, INFO_SETUP_INI);
    }

    void dragon::ClientHook::change_fps()
    {
        char GAME_SETUP_INI[] = ".\\ini\\GameSetUp.ini";

        int fps = GetPrivateProfileIntA("ScreenMode", "Fps", 60, GAME_SETUP_INI);

        int frameDelayMs = __max(5, 1000 / fps);

        memory::write(addr_fps1, &frameDelayMs, 1, NULL);
        memory::write(addr_fps2, &frameDelayMs, 1, NULL);
    }
}
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace LiveWallpaperX
{
    public static class Language
    {
        public static readonly Dictionary<string, LanguagePack> Languages = new Dictionary<string, LanguagePack>();

        public static void InstallLanguage(string langName, LanguagePack pack)
        {
            Languages.Add(langName.ToLowerInvariant(), pack);
        }

        public class LanguagePack
        {
            public string Title { get; set; }
            public string SetupTitle { get; set; }
            public string Menu { get; set; }
            public string SelectVideo { get; set; }
            public string AutoRun { get; set; }
            public string ForceExit { get; set; }
            public string Help { get; set; }
            public string Source { get; set; }
            public string VideoFiles { get; set; }
        }

        public static LanguagePack GetLanguagePack(string langName)
        {
            string name = langName;
            if (string.IsNullOrEmpty(name))
            {
                name = CultureInfo.CurrentUICulture.Name;
            }

            name = name.ToLowerInvariant();
            if (Languages.ContainsKey(name))
            {
                return Languages[name];
            }

            return Languages.FirstOrDefault().Value;
        }

        static Language()
        {
            var languagePack = new LanguagePack()
            {
                Title = "LiveWallpaperX",
                Menu = "Menu",
                SetupTitle = "LiveWallpaperX Setup",
                SelectVideo = "Select a Video File",
                VideoFiles = "Video Files",
                AutoRun = "Auto Start on Boot",
                ForceExit = "Force Exit Program",
                Help = "Shortcuts:\r\n\r\n\tCtrl + Alt + O : Show or hide the setup window;\r\n\r\n\tCtrl + Alt + P : Play or pause the video.",
                Source = "Source Code (GPL 2.0)"
            };
            InstallLanguage("en-US", languagePack);

            languagePack = new LanguagePack()
            {
                Title = "动态壁纸 X",
                SetupTitle = "动态壁纸 X - 设置",
                Menu = "菜单",
                SelectVideo = "选择一个视频文件",
                VideoFiles = "视频文件",
                AutoRun = "开机自启",
                ForceExit = "强行退出程序",
                Help = "快捷键:\r\n\r\n\tCtrl + Alt + O : 显示或隐藏设置界面；\r\n\r\n\tCtrl + Alt + P : 播放或暂停视频。",
                Source = "源代码 (GPL 2.0)"
            };
            InstallLanguage("zh-CN", languagePack);

            languagePack = new LanguagePack()
            {
                Title = "動態桌布 X",
                SetupTitle = "動態桌布 X - 設定",
                Menu = "選單",
                SelectVideo = "選擇一個影片檔案",
                VideoFiles = "影片檔案",
                AutoRun = "開機自動啟動",
                ForceExit = "強制結束程式",
                Help = "快速鍵:\r\n\r\n\tCtrl + Alt + O : 顯示或隱藏設定介面；\r\n\r\n\tCtrl + Alt + P : 播放或暫停影片。",
                Source = "原始碼 (GPL 2.0)"
            };
            InstallLanguage("zh-TW", languagePack);

            languagePack = new LanguagePack()
            {
                Title = "Fond d'écran dynamique X",
                SetupTitle = "Fond d'écran dynamique X - Paramètres",
                Menu = "Menu",
                SelectVideo = "Sélectionner un fichier vidéo",
                VideoFiles = "Fichiers vidéo",
                AutoRun = "Démarrage automatique",
                ForceExit = "Forcer la fermeture du programme",
                Help = "Raccourcis :\r\n\r\n\tCtrl + Alt + O : Afficher ou masquer l'interface des paramètres ;\r\n\r\n\tCtrl + Alt + P : Lire ou mettre en pause la vidéo.",
                Source = "Code source (GPL 2.0)"
            };
            InstallLanguage("fr-FR", languagePack);

            languagePack = new LanguagePack()
            {
                Title = "Papel de Parede Dinâmico X",
                SetupTitle = "Papel de Parede Dinâmico X - Configurações",
                Menu = "Menu",
                SelectVideo = "Selecionar um arquivo de vídeo",
                VideoFiles = "Arquivos de vídeo",
                AutoRun = "Iniciar automaticamente com o Windows",
                ForceExit = "Forçar saída do programa",
                Help = "Atalhos:\r\n\r\n\tCtrl + Alt + O : Mostrar ou ocultar a interface de configuração;\r\n\r\n\tCtrl + Alt + P : Reproduzir ou pausar o vídeo.",
                Source = "Código-fonte (GPL 2.0)"
            };
            InstallLanguage("pt-BR", languagePack);

            languagePack = new LanguagePack()
            {
                Title = "Dynamisches Hintergrundbild X",
                SetupTitle = "Dynamisches Hintergrundbild X - Einstellungen",
                Menu = "Menü",
                SelectVideo = "Videodatei auswählen",
                VideoFiles = "Videodateien",
                AutoRun = "Autostart",
                ForceExit = "Programm gewaltsam beenden",
                Help = "Tastenkürzel:\r\n\r\n\tStrg + Alt + O : Konfigurationsfenster ein- oder ausblenden;\r\n\r\n\tStrg + Alt + P : Video abspielen oder pausieren.",
                Source = "Quellcode (GPL 2.0)"
            };
            InstallLanguage("de-DE", languagePack);

            languagePack = new LanguagePack()
            {
                Title = "Динамічні шпалери X",
                SetupTitle = "Динамічні шпалери X - Налаштування",
                Menu = "Меню",
                SelectVideo = "Вибрати відеофайл",
                VideoFiles = "Відеофайли",
                AutoRun = "Автозапуск",
                ForceExit = "Примусовий вихід з програми",
                Help = "Гарячі клавіші:\r\n\r\n\tCtrl + Alt + O : Показати або приховати інтерфейс налаштувань;\r\n\r\n\tCtrl + Alt + P : Відтворити або призупинити відео.",
                Source = "Вихідний код (GPL 2.0)"
            };
            InstallLanguage("uk-UA", languagePack);

            languagePack = new LanguagePack()
            {
                Title = "動的壁紙 X",
                SetupTitle = "動的壁紙 X - 設定",
                Menu = "メニュー",
                SelectVideo = "動画ファイルを選択",
                VideoFiles = "動画ファイル",
                AutoRun = "自動起動",
                ForceExit = "プログラムを強制終了",
                Help = "ショートカット:\r\n\r\n\tCtrl + Alt + O : 設定画面の表示/非表示;\r\n\r\n\tCtrl + Alt + P : 動画の再生/一時停止。",
                Source = "ソースコード (GPL 2.0)"
            };
            InstallLanguage("ja-JP", languagePack);

            languagePack = new LanguagePack()
            {
                Title = "동적 배경화면 X",
                SetupTitle = "동적 배경화면 X - 설정",
                Menu = "메뉴",
                SelectVideo = "비디오 파일 선택",
                VideoFiles = "비디오 파일",
                AutoRun = "자동 시작",
                ForceExit = "프로그램 강제 종료",
                Help = "단축키:\r\n\r\n\tCtrl + Alt + O : 설정 창 표시/숨기기;\r\n\r\n\tCtrl + Alt + P : 동영상 재생/일시정지.",
                Source = "소스 코드 (GPL 2.0)"
            };
            InstallLanguage("ko-KR", languagePack);

            languagePack = new LanguagePack()
            {
                Title = "Fondo de pantalla dinámico X",
                SetupTitle = "Fondo de pantalla dinámico X - Configuración",
                Menu = "Menú",
                SelectVideo = "Seleccionar un archivo de video",
                VideoFiles = "Archivos de video",
                AutoRun = "Inicio automático",
                ForceExit = "Forzar salida del programa",
                Help = "Atajos:\r\n\r\n\tCtrl + Alt + O : Mostrar u ocultar la interfaz de configuración;\r\n\r\n\tCtrl + Alt + P : Reproducir o pausar el video.",
                Source = "Código fuente (GPL 2.0)"
            };
            InstallLanguage("es-ES", languagePack);

            languagePack = new LanguagePack()
            {
                Title = "Динамические обои X",
                SetupTitle = "Динамические обои X - Настройки",
                Menu = "Меню",
                SelectVideo = "Выбрать видеофайл",
                VideoFiles = "Видеофайлы",
                AutoRun = "Автозапуск",
                ForceExit = "Принудительный выход из программы",
                Help = "Горячие клавиши:\r\n\r\n\tCtrl + Alt + O : Показать или скрыть окно настроек;\r\n\r\n\tCtrl + Alt + P : Воспроизвести или приостановить видео.",
                Source = "Исходный код (GPL 2.0)"
            };
            InstallLanguage("ru-RU", languagePack);

            languagePack = new LanguagePack()
            {
                Title = "Sfondo dinamico X",
                SetupTitle = "Sfondo dinamico X - Impostazioni",
                Menu = "Menu",
                SelectVideo = "Seleziona un file video",
                VideoFiles = "File video",
                AutoRun = "Avvio automatico",
                ForceExit = "Forza chiusura del programma",
                Help = "Scorciatoie:\r\n\r\n\tCtrl + Alt + O : Mostra o nascondi l'interfaccia delle impostazioni;\r\n\r\n\tCtrl + Alt + P : Riproduci o metti in pausa il video.",
                Source = "Codice sorgente (GPL 2.0)"
            };
            InstallLanguage("it-IT", languagePack);
        }
    }
}

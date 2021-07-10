using System;
using System.Linq;
using ConsoleGUI.Data;
using Lanchat.Core.Network;
using Lanchat.Terminal.UserInterface.Controls;
using Lanchat.Terminal.UserInterface.Views;

namespace Lanchat.Terminal.UserInterface
{
    public class TabsManager
    {
        private readonly Tab homeViewTab;
        private readonly Tab mainViewTab;
        private readonly TabPanel tabPanel;

        public TabsManager(TabPanel tabPanel)
        {
            this.tabPanel = tabPanel;
            MainChatView = new ChatView();

            HomeView = new HomeView();
            homeViewTab = new Tab("Lanchat", HomeView);
            mainViewTab = new Tab("Lanchat", MainChatView);
            tabPanel.AddTab(homeViewTab);
            tabPanel.AddTab(new Tab("Detected users", new DetectedUsersView()));
            tabPanel.AddTab(new Tab("File transfer", new FileTransfersView()));
        }

        public ChatView MainChatView { get; }
        public HomeView HomeView { get; }

        public void ShowMainChatView()
        {
            if (tabPanel.Tabs[0].Content is HomeView)
            {
                tabPanel.ReplaceTab(homeViewTab, mainViewTab);
            }
        }

        public ChatView AddPrivateChatView(INode node)
        {
            var chatView = new ChatView(node);
            var chatTab = new Tab(node.User.Nickname, chatView) {Id = node.Id};
            tabPanel.AddUserTab(chatTab);
            return chatView;
        }
        
        public void ClosePrivateChatView(INode node)
        {
            var chatTab = tabPanel.Tabs.FirstOrDefault(x => x.Id == node.Id);
            tabPanel.RemoveUserTab(chatTab);
        }
        
        public void UpdateNickname(INode node)
        {
            var tab = tabPanel.Tabs.FirstOrDefault(x => x.Content is ChatView chatView && chatView.Node == node);
            tabPanel.UpdateUserTabHeader(tab, node.User.Nickname);
        }
        
        public DebugView AddDebugView()
        {
            var debugView = new DebugView();
            var debugTab = new Tab("Debug", debugView);
            tabPanel.AddTab(debugTab);
            return debugView;
        }
        
        public void WriteText(string text)
        {
            var writeable = tabPanel.CurrentTab.Content as IWriteable;
            writeable?.AddText(text, Color.White);
        }
        
        public void WriteError(string text)
        {
            var writeable = tabPanel.CurrentTab.Content as IWriteable;
            writeable?.AddText(text, ConsoleColor.Red);
        }
    }
}
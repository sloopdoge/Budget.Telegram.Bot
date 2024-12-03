using Budget.Telegram.Bot.Entity.Enums.Menus;

namespace Budget.Telegram.Bot.Business.Configs;

public class BotMenusConfig
{
    public Dictionary<StartMenuEnum, string> StartMenuButtons = new();
    public Dictionary<GroupMenuEnum, string> GroupMenuButtons = new();
    public Dictionary<BudgetMenuEnum, string> BudgetMenuButtons = new();

    public BotMenusConfig()
    {
        InitializeStartMenuButtons();
        InitializeGroupMenuButtons();
        InitializeBudgetMenuButtons();
    }
    
    private void InitializeStartMenuButtons()
    {
        StartMenuButtons.Add(StartMenuEnum.Groups, "Groups menu");
        StartMenuButtons.Add(StartMenuEnum.Budgets, "Budgets menu");
    }
    
    private void InitializeGroupMenuButtons()
    {
        GroupMenuButtons.Add(GroupMenuEnum.AddGroup, "Add group");
        GroupMenuButtons.Add(GroupMenuEnum.InviteToGroup, "Invite to group");
        GroupMenuButtons.Add(GroupMenuEnum.ListMyGroups, "Show all my groups");
    }
    
    private void InitializeBudgetMenuButtons()
    {
        BudgetMenuButtons.Add(BudgetMenuEnum.AddBudget, "Add budget");
        BudgetMenuButtons.Add(BudgetMenuEnum.EditBudget, "Edit budget");
        BudgetMenuButtons.Add(BudgetMenuEnum.ListMyBudgets, "Show all my groups");
    }
}
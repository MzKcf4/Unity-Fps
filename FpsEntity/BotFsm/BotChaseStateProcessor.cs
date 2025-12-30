public class BotChaseStateProcessor : AbstractBotStateProcessor
{
    private readonly ActionCooldown scanCooldown = new ActionCooldown { interval = 0.2f };
    private readonly ActionCooldown preAimTimer = new ActionCooldown { interval = 2f };

    public BotChaseStateProcessor(MzFpsBotBrain fpsBot, FpsHumanoidCharacter character, BotFsmDto botFsmDto) : base(fpsBot,character, botFsmDto)
    {
        isReactToTeammateKilled = false;
        isReactToUnknownDamage = true;
    }
    public override void EnterState()
    {
        if (botFsmDto.shootTargetLastSeenPosition != null)
        {
            fpsBotBrain.SetDestination(botFsmDto.shootTargetLastSeenPosition);
            fpsCharacter.AimAtPosition(botFsmDto.shootTargetLastSeenPosition);
            preAimTimer.StartCooldown();
        }
        else
        {
            ExitToState(BotStateEnum.Wandering);
        }
    }

    public override void ProcessState()
    {
        if (fpsBotBrain.IsReachedDesination())
        {
            ExitToState(BotStateEnum.Wandering);
            return;
        }

        // Scan other enemies while chasing
        if (!fpsBotBrain.aiIgnoreEnemy && scanCooldown.CanExecuteAfterDeltaTime(true))
        {
            FpsModel detectedEnemy = fpsBotBrain.ScanForShootTarget();
            if (detectedEnemy != null)
            {
                botFsmDto.shootTargetModel = detectedEnemy;
                ExitToState(BotStateEnum.Engage);
                return;
            }
        }

        if (preAimTimer.CanExecuteAfterDeltaTime())
        {
            fpsCharacter.AimAtMovementDirection();
        }
    }
}

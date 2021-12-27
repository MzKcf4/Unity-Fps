using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BotChaseStateProcessor : AbstractBotStateProcessor
{
    private readonly ActionCooldown scanCooldown = new ActionCooldown { interval = 0.2f };
    private readonly ActionCooldown preAimTimer = new ActionCooldown { interval = 2f };

    public BotChaseStateProcessor(FpsBot fpsBot, BotFsmDto botFsmDto) : base(fpsBot, botFsmDto)
    {
        isReactToTeammateKilled = false;
        isReactToUnknownDamage = true;
    }
    public override void EnterState()
    {
        if (botFsmDto.shootTargetLastSeenPosition != null)
        {
            fpsBot.SetDestination(botFsmDto.shootTargetLastSeenPosition);
            fpsBot.SetLookAtToPosition(botFsmDto.shootTargetLastSeenPosition);
            preAimTimer.StartCooldown();
        }
        else
        {
            ExitToState(BotStateEnum.Wandering);
        }
    }

    public override void ProcessState()
    {
        if (fpsBot.IsReachedDesination())
        {
            ExitToState(BotStateEnum.Wandering);
            return;
        }

        // Scan other enemies while chasing
        if (!fpsBot.aiIgnoreEnemy && scanCooldown.CanExecuteAfterDeltaTime(true))
        {
            FpsModel detectedEnemy = fpsBot.ScanForShootTarget();
            if (detectedEnemy != null)
            {
                botFsmDto.shootTargetModel = detectedEnemy;
                ExitToState(BotStateEnum.Engage);
                return;
            }
        }

        if (preAimTimer.CanExecuteAfterDeltaTime())
        {
            fpsBot.AlignLookAtWithMovementDirection();
        }
    }
}

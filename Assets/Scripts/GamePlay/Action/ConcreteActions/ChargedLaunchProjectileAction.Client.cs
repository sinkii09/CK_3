using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ChargedLaunchProjectileAction
{
    private List<SpecialFXGraphic> m_Graphics = new List<SpecialFXGraphic>();

    private bool m_ChargeEnded;

    public override bool OnStartClient(ClientCharacter clientCharacter)
    {
        base.OnStartClient(clientCharacter);

        m_Graphics = InstantiateSpecialFXGraphics(clientCharacter.transform, true);
        return true;
    }

    public override bool OnUpdateClient(ClientCharacter clientCharacter)
    {
        return !m_ChargeEnded;
    }

    public override void CancelClient(ClientCharacter clientCharacter)
    {
        if (!m_ChargeEnded)
        {
            foreach (var graphic in m_Graphics)
            {
                if (graphic)
                {
                    graphic.Shutdown();
                }
            }
        }
    }

    public override void OnStoppedChargingUpClient(ClientCharacter clientCharacter, float finalChargeUpPercentage)
    {
        m_ChargeEnded = true;
        foreach (var graphic in m_Graphics)
        {
            if (graphic)
            {
                graphic.Shutdown();
            }
        }
        m_Graphics.Clear();
    }
}

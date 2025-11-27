using UnityEngine;

public partial class BossController
{
    void InitHealthBar()
    {
        if (hpRoot != null) return;
        hpRoot = new GameObject("BossHPBar").transform;
        hpRoot.SetParent(transform);
        hpRoot.localPosition = new Vector3(-healthBarSize.x * 0.5f + healthBarOffset.x, healthBarOffset.y, 0f);
        var backGo = new GameObject("HPBarBack");
        backGo.transform.SetParent(hpRoot);
        backGo.transform.localPosition = Vector3.zero;
        hpBack = backGo.AddComponent<LineRenderer>();
        hpBack.useWorldSpace = false;
        hpBack.positionCount = 2;
        hpBack.startWidth = healthBarSize.y;
        hpBack.endWidth = healthBarSize.y;
        var backShader = Shader.Find("Sprites/Default");
        if (backShader != null)
        {
            var backMat = new Material(backShader);
            backMat.color = healthBackgroundColor;
            hpBack.material = backMat;
        }
        hpBack.startColor = healthBackgroundColor;
        hpBack.endColor = healthBackgroundColor;
        hpBack.sortingOrder = 2000;
        hpBack.SetPosition(0, Vector3.zero);
        hpBack.SetPosition(1, new Vector3(healthBarSize.x, 0f, 0f));
        var fillGo = new GameObject("HPBarFill");
        fillGo.transform.SetParent(hpRoot);
        fillGo.transform.localPosition = Vector3.zero;
        hpLine = fillGo.AddComponent<LineRenderer>();
        hpLine.useWorldSpace = false;
        hpLine.positionCount = 2;
        hpLine.startWidth = healthBarSize.y * 0.9f;
        hpLine.endWidth = healthBarSize.y * 0.9f;
        var lineShader = Shader.Find("Sprites/Default");
        if (lineShader != null)
        {
            var lineMat = new Material(lineShader);
            lineMat.color = healthColorFull;
            hpLine.material = lineMat;
        }
        hpLine.sortingOrder = 2001;
        UpdateHealthBar();
        UpdateHealthBarPosition();
    }

    void UpdateHealthBar()
    {
        if (!showHealthBar || hpLine == null) return;
        float pct = Mathf.Clamp01((float)health / Mathf.Max(1, maxHealth));
        hpLine.SetPosition(0, Vector3.zero);
        hpLine.SetPosition(1, new Vector3(healthBarSize.x * pct, 0f, 0f));
        var col = Color.Lerp(healthColorEmpty, healthColorFull, pct);
        hpLine.startColor = col;
        hpLine.endColor = col;
    }

    void UpdateHealthBarPosition()
    {
        if (hpRoot == null) return;
        Collider2D c = GetComponent<Collider2D>();
        if (c == null) c = GetComponentInChildren<Collider2D>();
        float yLocal = healthBarOffset.y;
        if (c != null)
        {
            float topWorldY = c.bounds.max.y + healthBarMargin;
            yLocal = topWorldY - transform.position.y;
        }
        float xCenterLocal = 0f;
        if (c != null)
        {
            xCenterLocal = c.bounds.center.x - transform.position.x;
        }
        hpRoot.localPosition = new Vector3(xCenterLocal - healthBarSize.x * 0.5f + healthBarOffset.x, yLocal, 0f);
    }
}

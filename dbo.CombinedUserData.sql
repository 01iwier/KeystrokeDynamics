CREATE VIEW [dbo].[CombinedUserData]
AS
SELECT
    u.[UserId],
    u.[UserName],
    AVG(ISNULL(m.[AvgSingleClick], 0.0)) AS [AvgSingleClick],
    AVG(ISNULL(m.[AvgDoubleClick], 0.0)) AS [AvgDoubleClick],
    AVG(ISNULL(m.[AvgScroll], 0.0)) AS [AvgScroll],
    AVG(ISNULL(m.[AvgHold], 0.0)) AS [AvgHold],
    AVG(ISNULL(m.[Count], 0.0)) AS [Count],
    AVG(ISNULL(k.[Accuracy], 0.0)) AS [Accuracy],
    AVG(ISNULL(k.[BackspaceCount], 0.0)) AS [BackspaceCount],
    AVG(ISNULL(k.[Errors], 0.0)) AS [Errors],
    AVG(ISNULL(k.[AvgHoldTime], 0.0)) AS [AvgHoldTime],
    AVG(ISNULL(k.[AvgSeekTime], 0.0)) AS [AvgSeekTime],
    AVG(ISNULL(k.[CPM], 0.0)) AS [AvgCPM]
FROM
    [dbo].[Users] u
    LEFT JOIN [dbo].[MouseData] m ON u.[UserId] = m.[UserId]
    LEFT JOIN [dbo].[KeyData] k ON u.[UserId] = k.[UserId]
WHERE
    m.[AvgSingleClick] IS NOT NULL AND
    m.[AvgDoubleClick] IS NOT NULL AND
    m.[AvgScroll] IS NOT NULL AND
    m.[AvgHold] IS NOT NULL AND
    m.[Count] IS NOT NULL AND
    k.[Accuracy] IS NOT NULL AND
    k.[BackspaceCount] IS NOT NULL AND
    k.[Errors] IS NOT NULL AND
    k.[AvgHoldTime] IS NOT NULL AND
    k.[AvgSeekTime] IS NOT NULL AND
    k.[CPM] IS NOT NULL
GROUP BY
    u.[UserId],
    u.[UserName];
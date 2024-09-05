CREATE VIEW [dbo].[CombinedUserData]
AS
SELECT
    u.[UserId],
    u.[UserName],
    AVG(m.[AvgSingleClick]) AS [AvgSingleClick],
    AVG(m.[AvgDoubleClick]) AS [AvgDoubleClick],
    AVG(m.[AvgScroll]) AS [AvgScroll],
    AVG(m.[AvgHold]) AS [AvgHold],
    AVG(m.[Count]) AS [Count],
    AVG(k.[Accuracy]) AS [Accuracy],
    AVG(k.[BackspaceCount]) AS [BackspaceCount],
    AVG(k.[Errors]) AS [Errors],
    AVG(k.[AvgHoldTime]) AS [AvgHoldTime],
    AVG(k.[AvgSeekTime]) AS [AvgSeekTime],
    AVG(k.[CPM]) AS [AvgCPM]
FROM
    [dbo].[Users] u
    LEFT JOIN [dbo].[MouseData] m ON u.[UserId] = m.[UserId]
    LEFT JOIN [dbo].[KeyData] k ON u.[UserId] = k.[UserId]
GROUP BY
    u.[UserId],
    u.[UserName];
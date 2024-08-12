using Backend.EntityFramework;

namespace Backend.tests.TestHelpers;
public class LibraryDbHelper : DbTestHelper<LibraryDbContext>
{
    //public async Task<List<Player>> SeedPlayersAsync()
    //{
    //    var players = new List<Player>
    //    {
    //        new() { Forename = "Player", Surname= "One" },
    //        new() { Forename = "Player", Surname = "Two" }
    //    };

    //    var context = NewContext();

    //    context.AddRange(players);
    //    await context.SaveChangesAsync();

    //    return players;
    //}

    //public async Task<List<Team>> SeedTeamsAsync()
    //{
    //    var teams = new List<Team>
    //    {
    //        new() { Name = "Team One", Competitions=Competition.HertsDiv2 | Competition.Oxon | Competition.Friendly },
    //        new() { Name = "Team Two", Competitions = Competition.HertsDiv2 | Competition.Oxon }
    //    };

    //    var context = NewContext();

    //    context.AddRange(teams);
    //    await context.SaveChangesAsync();

    //    return teams;
    //}

    //public async Task<List<Game>> SeedGamesAsync()
    //{
    //    var teams = await SeedTeamsAsync();

    //    var context = NewContext();

    //    var games = new List<Game>
    //    {
    //        new() {
    //            Date = new DateOnly(2024, 1, 1),
    //            Competition = Competition.HertsDiv2,
    //            HomeTeamId = teams[0].Id!.Value,
    //            AwayTeamId = teams[1].Id!.Value,
    //            HomeScore = 100,
    //            AwayScore = 90
    //        },
    //        new() { Date = new DateOnly(2024, 2, 3),
    //            Competition = Competition.Oxon,
    //            HomeTeamId = teams[1].Id!.Value,
    //            AwayTeamId = teams[0].Id!.Value,
    //            HomeScore = 100,
    //            AwayScore = 90
    //        }
    //    };

    //    context.AddRange(games);

    //    await context.SaveChangesAsync();

    //    FillInNavigationProps(games[0], teams);
    //    FillInNavigationProps(games[1], teams);

    //    return games;
    //}

    //private static void FillInNavigationProps(Game game, List<Team> teams)
    //{
    //    game.HomeTeam = teams.Single(x => x.Id == game.HomeTeamId);
    //    game.AwayTeam = teams.Single(x => x.Id == game.AwayTeamId);
    //}
}

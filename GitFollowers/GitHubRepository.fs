namespace GitFollowers

open SQLite
open System.Threading.Tasks
open FSharp.Control.Tasks

[<Interface>]
type IGitHubRepository =
    abstract Create : 'i -> ValueTask<bool>
    abstract Read : ValueTask<FollowerObject list>
    abstract Update : 'i -> ValueTask<int>
    abstract Delete : 'i -> ValueTask<int>

[<Struct>]
type AppEnv =
    interface IGitHubRepository with
        member _.Create(item) =
            vtask {
                let! result = SqliteConnection.connection.CreateTableAsync<FollowerObject>()
                let! insertAsync = SqliteConnection.connection.InsertAsync(item)

                return
                    insertAsync > 0
                    && result = CreateTableResult.Created
                    || result = CreateTableResult.Migrated
            }

        member _.Update(follower) =
            vtask {
                let! result = SqliteConnection.connection.UpdateAsync(follower)
                return result
            }

        member _.Delete(follower) =
            vtask {
                let! result = SqliteConnection.connection.DeleteAsync(follower)
                return result
            }

        member this.Read =
            vtask {
                let! result = SqliteConnection.connection.Table().ToListAsync()
                return result |> Seq.cast |> Seq.toList
            }


[<Interface>]
type IDataStore =
    abstract FavoriteRepository : IGitHubRepository

module Db =
    let createFavorite () =
        vtask {
            do!
                SqliteConnection.connection.CreateTableAsync<FollowerObject>()
                |> Async.AwaitTask
                |> Async.Ignore
        }

//    let updateUser (env: #IDb) user = env.FavoriteRepository.Execute(Sql.UpdateUser, user)

//    let private  convertToObject (item: Follower) =
//        let follower = FollowerObject()
//        follower.Id <- item.id
//        follower.Login <- item.login
//        follower.AvatarUrl <- item.avatar_url
//        follower

//    let insertFavorite(follower: Follower) =
//            vtask {
//                let! database = SqliteConnection.connection
//                let obj = convertToObject follower
//
//                do! database.InsertAsync(obj)
//                    |> Async.AwaitTask
//                    |> Async.Ignore
//
//                let! rowIdObj =
//                    database.ExecuteScalarAsync("select last_insert_rowid()", [||])
//                    |> Async.AwaitTask
//
//                let rowId = rowIdObj |> int
//
//                return { follower with id = rowId }
//            }

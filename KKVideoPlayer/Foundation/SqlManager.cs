namespace KKVideoPlayer.Foundation
{
    using System;
    using System.Data.SQLite;
    using System.IO;

    /// <summary>
    /// SQLManager.
    /// </summary>
    public static class SqlManager
    {
        /// <summary>
        /// SQL Blob to bytes array.
        /// </summary>
        /// <param name="rdr">rdr is an output from SQLReader.</param>
        /// <returns>byte array.</returns>
        public static byte[] GetBytes(SQLiteDataReader rdr)
        {
            const int CHUNK_SIZE = 2 * 1024;
            byte[] buffer = new byte[CHUNK_SIZE];
            long bytesRead;
            long fieldOffset = 0;
            using (MemoryStream stream = new MemoryStream())
            {
                while ((bytesRead = rdr.GetBytes(0, fieldOffset, buffer, 0, buffer.Length)) > 0)
                {
                    stream.Write(buffer, 0, (int)bytesRead);
                    fieldOffset += bytesRead;
                }

                return stream.ToArray();
            }
        }

        /// <summary>
        ///  check if deepdark.db exists. If it doesn't, then create database.
        /// </summary>
        /// <param name="fileDirectory">The directory that you want to check.</param>
        /// <returns>boolean.</returns>
        public static bool CheckOrCreateDb(string fileDirectory)
        {
            if (!File.Exists($"{fileDirectory}/deepdark.db"))
            {
                using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + $"{fileDirectory}/deepdark.db"))
                {
                    conn.Open();

                    using (SQLiteTransaction tr = conn.BeginTransaction())
                    {
                        string sql = @"CREATE TABLE Config(
                                    key TEXT PRIMARY KEY NOT NULL,
                                    strValue TEXT,
                                    intValue INTEGER,
                                    realValue REAL
                                    );
                                    CREATE TABLE Cover(
                                    dvdId TEXT,
                                    image BLOB
                                    );
                                    CREATE TABLE Dvd(
                                    dvdId TEXT PRIMARY KEY NOT NULL,
                                    dvdTitle TEXT,
                                    actors TEXT, 
                                    genres TEXT,
                                    releaseDate TEXT,
                                    director TEXT,
                                    company TEXT,
                                    series TEXT,
                                    metaSource TEXT
                                    );
                                    CREATE TABLE Favorites(
                                    uid INTEGER PRIMARY KEY AUTOINCREMENT,
                                    filepath TEXT NOT NULL,
                                    start REAL,
                                    length REAL DEFAULT 0,
                                    thumb BLOB
                                    );
                                    CREATE TABLE Files(
                                    uid INTEGER PRIMARY KEY AUTOINCREMENT,
                                    filepath TEXT NOT NULL UNIQUE,
                                    dvdId TEXT,
                                    stars REAL DEFAULT 0,
                                    dbDate TEXT,
                                    fileDate TEXT,
                                    playDate TEXT,
                                    fileSize INTEGER DEFAULT 0,
                                    hashTag TEXT,
                                    thumb BLOB,
                                    count INTEGER DEFAULT 0,
                                    metaData TEXT,
                                    trash INTEGER DEFAULT 0,
                                    dvdIsNotExists INTEGER DEFAULT 0
                                    );
                                    CREATE TABLE Log(
                                    uid INTEGER PRIMARY KEY AUTOINCREMENT,
                                    name NOT NULL,
                                    data TEXT,
                                    date TEXT
                                    );";
                        using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                        {
                            cmd.Transaction = tr;

                            cmd.ExecuteNonQuery();
                        }

                        tr.Commit();
                    }

                    conn.Close();
                }

                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
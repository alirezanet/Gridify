using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EntityFrameworkIntegrationTests.cs
{
   public class SuppressConnectionInterceptor : DbConnectionInterceptor
   {
      public override ValueTask<InterceptionResult> ConnectionOpeningAsync(DbConnection connection, ConnectionEventData eventData,
         InterceptionResult result,
         CancellationToken cancellationToken = new())
      {
         result = InterceptionResult.Suppress();
         return base.ConnectionOpeningAsync(connection, eventData, result, cancellationToken);
      }

      public override InterceptionResult ConnectionOpening(DbConnection connection, ConnectionEventData eventData, InterceptionResult result)
      {
         result = InterceptionResult.Suppress();
         return base.ConnectionOpening(connection, eventData, result);
      }
   }

   public class EmptyMessageDataReader : DbDataReader
   {
   
      private readonly List<User> _users = new List<User>();

      public EmptyMessageDataReader()
      {
      }

      public override int FieldCount
         => 0;

      public override int RecordsAffected
         => 0;

      public override bool HasRows
         => false;

      public override bool IsClosed
         => true;

      public override int Depth
         => 0;

      public override bool Read()
         => false;

      public override int GetInt32(int ordinal)
         => 0;

      public override bool IsDBNull(int ordinal)
         => false;

      public override string GetString(int ordinal)
         => "suppressed message";

      public override bool GetBoolean(int ordinal)
         => true;

      public override byte GetByte(int ordinal)
         => 0;

      public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
         => 0;

      public override char GetChar(int ordinal)
         => '\0';

      public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
         => 0;

      public override string GetDataTypeName(int ordinal)
         => string.Empty;

      public override DateTime GetDateTime(int ordinal)
         => DateTime.Now;

      public override decimal GetDecimal(int ordinal)
         => 0;

      public override double GetDouble(int ordinal)
         => 0;

      public override Type GetFieldType(int ordinal)
         => typeof(User);

      public override float GetFloat(int ordinal)
         => 0;

      public override Guid GetGuid(int ordinal)
         => Guid.Empty;

      public override short GetInt16(int ordinal)
         => 0;

      public override long GetInt64(int ordinal)
         => 0;

      public override string GetName(int ordinal)
         => "";

      public override int GetOrdinal(string name)
         => 0;

      public override object GetValue(int ordinal)
         => new object();

      public override int GetValues(object[] values)
         => 0;

      public override object this[int ordinal]
         => new object();

      public override object this[string name]
         => new object();

      public override bool NextResult()
         => false;

      public override IEnumerator GetEnumerator()
         => _users.GetEnumerator();
   }

   public class SuppressCommandResultInterceptor : DbCommandInterceptor
   {
      public override InterceptionResult<DbDataReader> ReaderExecuting(
         DbCommand command,
         CommandEventData eventData,
         InterceptionResult<DbDataReader> result)
      {
         result = InterceptionResult<DbDataReader>.SuppressWithResult(new EmptyMessageDataReader());

         return result;
      }

      public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
         DbCommand command,
         CommandEventData eventData,
         InterceptionResult<DbDataReader> result,
         CancellationToken cancellationToken = default)
      {
         result = InterceptionResult<DbDataReader>.SuppressWithResult(new EmptyMessageDataReader());

         return new ValueTask<InterceptionResult<DbDataReader>>(result);
      }
   }
}
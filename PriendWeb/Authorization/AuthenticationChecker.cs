using Npgsql;

namespace PriendWeb.Authorization
{
    /// <summary>
    /// 사용자의 권한을 확인하는 메서드의 정적 클래스
    /// </summary>
    public static class AuthorizationChecker
    {
        /// <summary>
        /// 사용자 인증 토큰의 유효성을 검사한다.
        /// </summary>
        /// <param name="cmd">DB 커맨드</param>
        /// <param name="id">64비트 정수 사용자 식별자</param>
        /// <param name="token">문자열 사용자 인증 토큰</param>
        /// <returns>정상적이면 true, 아니면 false</returns>
        public static bool ValidateToken(NpgsqlCommand cmd, long id, string token)
        {
            cmd.CommandText = $"SELECT id FROM account WHERE id={id} AND auth_token='{token}';";
            using (var reader = cmd.ExecuteReader())
            {
                return reader.HasRows;
            }
        }

        /// <summary>
        /// 그룹에 대한 사용자의 접근 가능성을 검사한다.
        /// </summary>
        /// <param name="cmd">DB 커맨드</param>
        /// <param name="id">64비트 정수 사용자 식별자</param>
        /// <param name="groupId">32비트 그룹 식별자</param>
        /// <returns>접근 가능하면 true, 아니면 false</returns>
        public static bool CheckAuthorizationOnGroup(NpgsqlCommand cmd, long id, int groupId)
        {
            cmd.CommandText = $"SELECT group_id FROM participates WHERE group_id={groupId} AND account_id={id};";
            using (var reader = cmd.ExecuteReader())
            {
                return reader.HasRows;
            }
        }

        /// <summary>
        /// 반려동물에 대한 사용자의 접근 가능성을 감사한다.
        /// </summary>
        /// <param name="cmd">DB 커맨드</param>
        /// <param name="id">64비트 정수 사용자 식별자</param>
        /// <param name="groupId">64비트 반려동물 식별자</param>
        /// <returns>접근 가능하면 true, 아니면 false</returns>
        public static bool CheckAuthorizationOnAnimal(NpgsqlCommand cmd, long id, long animalId)
        {
            cmd.CommandText =
                $"SELECT managed.group_id FROM managed, participates " +
                $"WHERE managed.group_id=participates.group_id " +
                $"AND managed.pet_id={animalId} " +
                $"AND participates.account_id={id};";
            using (var reader = cmd.ExecuteReader())
            {
                return reader.HasRows;
            }
        }
    }
}

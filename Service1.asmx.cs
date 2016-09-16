using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Web;
using System.Web.Services;
using Oracle.DataAccess.Client;

namespace is2otodoke
{
	/// <summary>
	/// [is2otodoke]
	/// </summary>
	//--------------------------------------------------------------------------
	// 修正履歴
	//--------------------------------------------------------------------------
	// ADD 2007.04.28 東都）高木 オブジェクトの破棄
	//	disposeReader(reader);
	//	reader = null;
	//--------------------------------------------------------------------------
	// DEL 2007.05.10 東都）高木 未使用関数のコメント化
	//	logFileOpen(sUser);
	//	userCheck2(conn2, sUser);
	//	logFileClose();
	//--------------------------------------------------------------------------
	// MOD 2008.06.11 kcl)森本 着店コード検索方法の変更に伴い特殊ＣＤの登録を追加
	//  Get_Stodokesaki
	//  Get_csvwrite
	//  Get_csvwrite2
	//  Ins_todokesaki
	//  Upd_todokesaki
	//  Ins_uploadData
	//--------------------------------------------------------------------------
	// ADD 2008.10.16 kcl)森本 着店コード存在チェック追加 
	//--------------------------------------------------------------------------
	// ADD 2009.01.29 東都）高木 一覧に名前２、住所２、住所３を追加 
	// ADD 2009.01.29 東都）高木 一覧のソート順に[荷受人ＣＤ]を追加 
	// ADD 2009.01.29 東都）高木 一覧のソート順に[名前２]を追加 
	// ADD 2009.01.30 東都）高木 [名前３]に最終利用年月を更新 
	//--------------------------------------------------------------------------
	// MOD 2010.02.02 東都）高木 一覧に登録日時、更新日時、出荷使用日を追加 
	// MOD 2010.02.02 東都）高木 荷受人マスタの[登録ＰＧ]に出荷使用日を更新 
	// MOD 2010.02.03 東都）高木 検索条件に更新日を追加 
	// MOD 2010.02.03 東都）高木 ＣＳＶ出力時に文字項目の前に[']を追加 
	// MOD 2010.03.11 東都）高木 ＣＳＶ取込時の郵便番号１００件一括チェックを追加 
	//保留 MOD 2010.04.13 東都）高木 郵便番号が削除された時の障害対応 
	// MOD 2010.05.10 東都）高木 ＰＧ障害の修正 
	// MOD 2010.06.03 東都）高木 複数件削除機能の高速化 
	//--------------------------------------------------------------------------
	// MOD 2011.01.18 東都）高木 住所名前の前SPACEをつめない 
	//--------------------------------------------------------------------------
	[System.Web.Services.WebService(
		 Namespace="http://Walkthrough/XmlWebServices/",
		 Description="is2otodoke")]

	public class Service1 : is2common.CommService
	{
// ADD 2009.01.29 東都）高木 一覧に名前２、住所２、住所３を追加 START
		private static string sCRLF = "\\r\\n";
// ADD 2009.01.29 東都）高木 一覧に名前２、住所２、住所３を追加 END
		private static string sSepa = "|";
		private static string sKanma = ",";
		private static string sDbl = "\"";
// MOD 2010.02.03 東都）高木 ＣＳＶ出力時に文字項目の前に[']を追加 START
		private static string sSng = "'";
// MOD 2010.02.03 東都）高木 ＣＳＶ出力時に文字項目の前に[']を追加 END

		public Service1()
		{
			//CODEGEN: この呼び出しは、ASP.NET Web サービス デザイナで必要です。
			InitializeComponent();

			connectService();
		}

		#region コンポーネント デザイナで生成されたコード 
		
		//Web サービス デザイナで必要です。
		private IContainer components = null;
				
		/// <summary>
		/// デザイナ サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディタで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{
		}

		/// <summary>
		/// 使用されているリソースに後処理を実行します。
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if(disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);		
		}
		
		#endregion

		/*********************************************************************
		 * 届け先一覧取得
		 * 引数：会員ＣＤ、部門ＣＤ、カナ、荷受人ＣＤ
		 * 戻値：ステータス、一覧（名前１、住所１、荷受人ＣＤ、電話番号、カナ略称）
		 *
		 *********************************************************************/
// MOD 2005.05.11 東都）高木 ORA-03113対策？ START
//		private static string GET_OTODOKE_SELECT
//			= "SELECT 名前１,住所１,荷受人ＣＤ,'(' || TRIM(電話番号１) || ')' \n"
//			+       " || TRIM(電話番号２) || '-' || TRIM(電話番号３),カナ略称 \n"
//			+ " FROM ＳＭ０２荷受人 \n";
		private static string GET_OTODOKE_SELECT
			= "SELECT /*+ INDEX(ＳＭ０２荷受人 SM02PKEY) */ \n"
			+ " 名前１, 住所１, 荷受人ＣＤ, 電話番号１, \n"
			+ " 電話番号２, 電話番号３, カナ略称 \n"
// ADD 2009.01.29 東都）高木 一覧に名前２、住所２、住所３を追加 START
			+ ", 名前２, 住所２, 住所３ \n"
// ADD 2009.01.29 東都）高木 一覧に名前２、住所２、住所３を追加 END
// MOD 2010.02.02 東都）高木 一覧に登録日時、更新日時、出荷使用日を追加 START
			+ ", 郵便番号, 特殊計, \"メールアドレス\", 住所ＣＤ, 特殊ＣＤ \n"
			+ ", TO_CHAR(登録日時), TO_CHAR(更新日時), 登録ＰＧ \n"
// MOD 2010.02.02 東都）高木 一覧に登録日時、更新日時、出荷使用日を追加 END
			+ " FROM ＳＭ０２荷受人 \n";
// MOD 2005.05.11 東都）高木 ORA-03113対策？ END

		[WebMethod]
		public String[] Get_todokesaki(string[] sUser, string sKCode, string sBCode, string sKana, string sTCode)
			{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//			logFileOpen(sUser);
			logWriter(sUser, INF, "届け先一覧取得開始");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];
			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//// ADD 2005.05.23 東都）伊賀 会員チェック追加 START
//			// 会員チェック
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//// ADD 2005.05.23 東都）伊賀 会員チェック追加 END

			StringBuilder sbQuery = new StringBuilder(1024);
			StringBuilder sbRet = new StringBuilder(1024);
			try
			{
				sbQuery.Append(GET_OTODOKE_SELECT);
				sbQuery.Append(" WHERE 会員ＣＤ = '" + sKCode + "' \n");
				sbQuery.Append("   AND 部門ＣＤ = '" + sBCode + "' \n");
				if(sKana.Length > 0 && sTCode.Length == 0)
				{
					sbQuery.Append(" AND カナ略称 LIKE '%"+ sKana + "%' \n");
				}
				if(sTCode.Length > 0 && sKana.Length == 0)
				{
					sbQuery.Append(" AND 荷受人ＣＤ LIKE '"+ sTCode + "%' \n");
				}
				if(sTCode.Length > 0 && sKana.Length > 0)
				{
					sbQuery.Append(" AND カナ略称 LIKE '%"+ sKana + "%' \n");
					sbQuery.Append(" AND 荷受人ＣＤ LIKE '"+ sTCode + "%' \n");
				}
				sbQuery.Append(" AND 削除ＦＧ = '0' \n");
				sbQuery.Append(" ORDER BY 名前１ \n");
// ADD 2009.01.29 東都）高木 一覧のソート順に[名前２]を追加 START
				sbQuery.Append(", 名前２ ");
// ADD 2009.01.29 東都）高木 一覧のソート順に[名前２]を追加 END
// ADD 2009.01.29 東都）高木 一覧のソート順に[荷受人ＣＤ]を追加 START
				sbQuery.Append(", 荷受人ＣＤ \n");
// ADD 2009.01.29 東都）高木 一覧のソート順に[荷受人ＣＤ]を追加 END

				OracleDataReader reader = CmdSelect(sUser, conn2, sbQuery);
				while (reader.Read())
				{
					sbRet = new StringBuilder(1024);

// MOD 2011.01.18 東都）高木 住所名前の前SPACEをつめない START
//					sbRet.Append(sSepa + reader.GetString(0).Trim());
//					sbRet.Append(sSepa + reader.GetString(1).Trim());
					sbRet.Append(sSepa + reader.GetString(0).TrimEnd()); // 名前１
					sbRet.Append(sSepa + reader.GetString(1).TrimEnd()); // 住所１
//保留 MOD 2011.01.18 東都）高木 住所名前の前SPACEをつめない END
					sbRet.Append(sSepa + reader.GetString(2).Trim());
// MOD 2005.05.11 東都）高木 ORA-03113対策？ START
//					sbRet.Append(sSepa + reader.GetString(3));
//					sbRet.Append(sSepa + reader.GetString(4).Trim());
					sbRet.Append(sSepa + "(" + reader.GetString(3).Trim() + ")"
										+ reader.GetString(4).Trim() + "-"
										+ reader.GetString(5).Trim());	// 電話番号
// MOD 2011.01.18 東都）高木 住所名前の前SPACEをつめない START
//					sbRet.Append(sSepa + reader.GetString(6).Trim());	// カナ略称
					sbRet.Append(sSepa + reader.GetString(6).TrimEnd());// カナ略称
// MOD 2011.01.18 東都）高木 住所名前の前SPACEをつめない END
// MOD 2005.05.11 東都）高木 ORA-03113対策？ END

					sList.Add(sbRet);
				}
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				disposeReader(reader);
				reader = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END

				sRet = new string[sList.Count + 1];
				if(sList.Count == 0) 
					sRet[0] = "該当データがありません";
				else
				{
					sRet[0] = "正常終了";
					int iCnt = 1;
					IEnumerator enumList = sList.GetEnumerator();
					while(enumList.MoveNext())
					{
						sRet[iCnt] = enumList.Current.ToString();
						iCnt++;
					}
				}
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "サーバエラー：" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				conn2 = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
			}
			
			return sRet;
		}

		/*********************************************************************
		 * 届け先データ取得
		 * 引数：会員ＣＤ、部門ＣＤ、荷受人ＣＤ
		 * 戻値：ステータス、カナ略称、電話番号、郵便番号、住所、名前、特殊計
		 *		 メールアドレス、更新日時
		 *********************************************************************/
// MOD 2007.02.13 東都）高木 ＮＵＬＬエラー対策 START
//// ADD 2005.05.11 東都）高木 ORA-03113対策？ START
//		private static string GET_STODOKESAKI_SELECT
//			= "SELECT カナ略称, 電話番号１, 電話番号２, 電話番号３, \n"
//			+ " 郵便番号, 住所１, 住所２, 住所３, \n"
//			+ " 名前１, 名前２, 特殊計, \"メールアドレス\", 更新日時 \n"
//			+ " FROM ＳＭ０２荷受人 \n";
//// ADD 2005.05.11 東都）高木 ORA-03113対策？ END
		private static string GET_STODOKESAKI_SELECT
			= "SELECT カナ略称, 電話番号１, 電話番号２, 電話番号３, \n"
			+ " 郵便番号, 住所１, 住所２, 住所３, \n"
			+ " 名前１, 名前２, 特殊計, \"メールアドレス\", TO_CHAR(更新日時) \n"
// ADD 2008.06.11 kcl)森本 着店コード検索方法の変更 START
			+ ",住所ＣＤ, 特殊ＣＤ \n"
// ADD 2008.06.11 kcl)森本 着店コード検索方法の変更 END
			+ " FROM ＳＭ０２荷受人 \n";
// MOD 2007.02.13 東都）高木 ＮＵＬＬエラー対策 END
		[WebMethod]
		public String[] Get_Stodokesaki(string[] sUser, string sKCode,string sBCode,string sTCode)
		{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//			logFileOpen(sUser);
			logWriter(sUser, INF, "届け先情報取得開始");

			OracleConnection conn2 = null;
// MOD 2008.06.11 kcl)森本 着店コード検索方法の変更 START
//			string[] sRet = new string[16];
			string[] sRet = new string[18];
// MOD 2008.06.11 kcl)森本 着店コード検索方法の変更 END
			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//// ADD 2005.05.23 東都）伊賀 会員チェック追加 START
//			// 会員チェック
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//// ADD 2005.05.23 東都）伊賀 会員チェック追加 END

			try
			{
// MOD 2005.05.11 東都）高木 ORA-03113対策？ START
//				string cmdQuery = "SELECT カナ略称,電話番号１,電話番号２,電話番号３,"
//					+ "SUBSTR(郵便番号,1,3),SUBSTR(郵便番号,4,4),住所１,住所２,住所３,"
//					+ "名前１,名前２,特殊計, \"メールアドレス\", TO_CHAR(更新日時) \n"
//					+ "  FROM ＳＭ０２荷受人 \n"
//					+ " WHERE 荷受人ＣＤ = '" + sTCode + "' AND 会員ＣＤ = '" + sKCode + "' \n"
//					+ "   AND 部門ＣＤ   = '" + sBCode + "' AND 削除ＦＧ = '0'";
				string cmdQuery = GET_STODOKESAKI_SELECT
					+ " WHERE 荷受人ＣＤ = '" + sTCode + "' \n"
					+ " AND 会員ＣＤ = '" + sKCode + "' \n"
					+ " AND 部門ＣＤ = '" + sBCode + "' \n"
					+ " AND 削除ＦＧ = '0' \n";
// MOD 2005.05.11 東都）高木 ORA-03113対策？ END

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);

				bool bRead = reader.Read();
				if(bRead == true)
				{
// MOD 2005.05.11 東都）高木 ORA-03113対策？ START
//					for(int iCnt = 1; iCnt < 15; iCnt++)
//					{
//						sRet[iCnt] = reader.GetString(iCnt - 1).Trim();
//					}
// MOD 2011.01.18 東都）高木 住所名前の前SPACEをつめない START
//					sRet[1]  = reader.GetString(0).Trim();
					sRet[1]  = reader.GetString(0).TrimEnd(); // カナ略称
// MOD 2011.01.18 東都）高木 住所名前の前SPACEをつめない END
					sRet[2]  = reader.GetString(1).Trim();
					sRet[3]  = reader.GetString(2).Trim();
					sRet[4]  = reader.GetString(3).Trim();
					sRet[5]  = reader.GetString(4).Trim();	// 郵便番号
					sRet[6]  = "";
					if(sRet[5].Length > 3)
					{
						sRet[6]  = sRet[5].Substring(3);
						sRet[5]  = sRet[5].Substring(0,3);
					}
// MOD 2011.01.18 東都）高木 住所名前の前SPACEをつめない START
//					sRet[7]  = reader.GetString(5).Trim();
//					sRet[8]  = reader.GetString(6).Trim();
//					sRet[9]  = reader.GetString(7).Trim();
//					sRet[10] = reader.GetString(8).Trim();
//					sRet[11] = reader.GetString(9).Trim();
					sRet[7]  = reader.GetString(5).TrimEnd(); // 住所１
					sRet[8]  = reader.GetString(6).TrimEnd(); // 住所２
					sRet[9]  = reader.GetString(7).TrimEnd(); // 住所３
					sRet[10] = reader.GetString(8).TrimEnd(); // 名前１
					sRet[11] = reader.GetString(9).TrimEnd(); // 名前２
// MOD 2011.01.18 東都）高木 住所名前の前SPACEをつめない END
					sRet[12] = reader.GetString(10).Trim();
					sRet[13] = reader.GetString(11).Trim();
// MOD 2007.02.13 東都）高木 ＮＵＬＬエラー対策 START
//					sRet[14] = reader.GetDecimal(12).ToString().Trim();	// 更新日時
					sRet[14] = reader.GetString(12).Trim();	// 更新日時
// MOD 2007.02.13 東都）高木 ＮＵＬＬエラー対策 END
// MOD 2005.05.11 東都）高木 ORA-03113対策？ END
					sRet[0] = "更新";
					sRet[15] = "U";
// ADD 2008.06.11 kcl)森本 着店コード検索方法の変更 START
					sRet[16] = reader.GetString(13).Trim();
					sRet[17] = reader.GetString(14).Trim();
// ADD 2008.06.11 kcl)森本 着店コード検索方法の変更 END
				}
				else
				{
					sRet[0] = "登録";
					sRet[15] = "I";
				}
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				disposeReader(reader);
				reader = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END

			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "サーバエラー：" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				conn2 = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
			}
			
			return sRet;
		}

		/*********************************************************************
		 * 届け先データ更新
		 * 引数：会員ＣＤ、部門ＣＤ、荷受人ＣＤ...
		 * 戻値：ステータス
		 *
		 *********************************************************************/
		[WebMethod]
		public String[] Upd_todokesaki(string[] sUser, string[] sData)
		{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//			logFileOpen(sUser);
			logWriter(sUser, INF, "届け先更新開始");

			OracleConnection conn2 = null;
			string[] sRet = new string[5];
			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//// ADD 2005.05.23 東都）伊賀 会員チェック追加 START
//			// 会員チェック
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//// ADD 2005.05.23 東都）伊賀 会員チェック追加 END

// ADD 2006.07.10 東都）高木 住所ＣＤの追加 START
			string s住所ＣＤ = " ";
			if(sData.Length >= 20 && sData[19].Length > 0){
				s住所ＣＤ = sData[19];
			}
// ADD 2006.07.10 東都）高木 住所ＣＤの追加 END
// ADD 2008.06.11 kcl)森本 着店コード検索方法の変更 START
			string s特殊ＣＤ = " ";
			if (sData.Length >= 21 && sData[20].Length > 0) 
			{
				s特殊ＣＤ = sData[20];
			}
// ADD 2008.06.11 kcl)森本 着店コード検索方法の変更 END

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			try
			{
				string cmdQuery 
					= "UPDATE ＳＭ０２荷受人 \n"
					+    "SET カナ略称           = '" + sData[1]  +"', \n"
					+        "電話番号１         = '" + sData[2]  +"', \n"
					+        "電話番号２         = '" + sData[3]  +"', \n"
					+        "電話番号３         = '" + sData[4]  +"', \n"
					+        "郵便番号           = '" + sData[5] + sData[6] +"', \n"
					+        "住所１             = '" + sData[7]  +"', \n"
					+        "住所２             = '" + sData[8]  +"', \n"
					+        "住所３             = '" + sData[9]  +"', \n"
					+        "名前１             = '" + sData[10] +"', \n"
					+        "名前２             = '" + sData[11] +"', \n"
// MOD 2010.02.02 東都）高木 荷受人マスタの[登録ＰＧ]に出荷使用日を更新 START
//// ADD 2009.01.30 東都）高木 [名前３]に最終利用年月を更新 START
//					+        "名前３             = TO_CHAR(SYSDATE,'YYYYMM'), \n"
//// ADD 2009.01.30 東都）高木 [名前３]に最終利用年月を更新 END
// MOD 2010.02.02 東都）高木 荷受人マスタの[登録ＰＧ]に出荷使用日を更新 END
// MOD 2006.07.10 東都）高木 住所ＣＤの追加 START
					+        "住所ＣＤ           = '" + s住所ＣＤ +"', \n"
// MOD 2006.07.10 東都）高木 住所ＣＤの追加 END
// ADD 2008.06.11 kcl)森本 着店コード検索方法の変更 START
					+        "特殊ＣＤ           = '" + s特殊ＣＤ +"', \n"
// ADD 2008.06.11 kcl)森本 着店コード検索方法の変更 END
					+        "特殊計             = '" + sData[12] +"', \n"
					+        "\"メールアドレス\" = '" + sData[13] +"', \n"
					+        "削除ＦＧ           = '0', \n"
// MOD 2010.02.02 東都）高木 荷受人マスタの[登録ＰＧ]に出荷使用日を更新 START
					+        "登録ＰＧ           = ' ', \n"
// MOD 2010.02.02 東都）高木 荷受人マスタの[登録ＰＧ]に出荷使用日を更新 END
					+        "更新ＰＧ           = '" + sData[14] +"', \n"
					+        "更新者             = '" + sData[15] +"', \n"
					+        "更新日時           =  TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') \n"
					+ " WHERE 会員ＣＤ           = '" + sData[16] +"' \n"
					+ "   AND 部門ＣＤ           = '" + sData[17] +"' \n"
					+ "   AND 荷受人ＣＤ         = '" + sData[0] +"' \n"
					+ "   AND 更新日時           =  " + sData[18] +"";

				int iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);
				tran.Commit();
				if(iUpdRow == 0)
					sRet[0] = "データ編集中に他の端末より更新されています。\r\n再度、最新データを呼び出して更新してください。";
				else				
					sRet[0] = "正常終了";
			}
			catch (OracleException ex)
			{
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				tran.Rollback();
				sRet[0] = "サーバエラー：" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				conn2 = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
			}
			
			return sRet;
		}

		/*********************************************************************
		 * 届け先データ登録
		 * 引数：会員ＣＤ、部門ＣＤ、荷受人ＣＤ...
		 * 戻値：ステータス
		 *
		 *********************************************************************/
		[WebMethod]
		public String[] Ins_todokesaki(string[] sUser, string[] sData)
		{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//			logFileOpen(sUser);
			logWriter(sUser, INF, "届け先登録開始");

			OracleConnection conn2 = null;
			string[] sRet = new string[5];
			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//// ADD 2005.05.23 東都）伊賀 会員チェック追加 START
//			// 会員チェック
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//// ADD 2005.05.23 東都）伊賀 会員チェック追加 END

// ADD 2006.07.10 東都）高木 住所ＣＤの追加 START
			string s住所ＣＤ = " ";
			if(sData.Length >= 20 && sData[19].Length > 0){
				s住所ＣＤ = sData[19];
			}
// ADD 2006.07.10 東都）高木 住所ＣＤの追加 END
// ADD 2008.06.11 kcl)森本 着店コード検索方法の変更 START
			string s特殊ＣＤ = " ";
			if (sData.Length >= 21 && sData[20].Length > 0) 
			{
				s特殊ＣＤ = sData[20];
			}
// ADD 2008.06.11 kcl)森本 着店コード検索方法の変更 END

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			try
			{
				string cmdQuery 
					= "DELETE FROM ＳＭ０２荷受人 \n"
					+ " WHERE 会員ＣＤ           = '" + sData[16] +"' \n"
					+ "   AND 部門ＣＤ           = '" + sData[17] +"' \n"
					+ "   AND 荷受人ＣＤ         = '" + sData[0] +"' \n"
					+ "   AND 削除ＦＧ           = '1'";

				int iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);

				cmdQuery 
					= "INSERT INTO ＳＭ０２荷受人 \n"
					+ "VALUES ('" + sData[16] +"','" + sData[17] +"','" + sData[0] +"','" + sData[2] +"','" + sData[3] +"', \n"
					+         "'" + sData[4] +"',' ',' ',' ','" + sData[7] +"','" + sData[8] +"', \n"
// MOD 2006.07.10 東都）高木 住所ＣＤの追加 START
//					+         "'" + sData[9] +"','" + sData[10] +"','" + sData[11] +"',' ','" + sData[5] + sData[6] +"',' ', \n"
// MOD 2010.02.02 東都）高木 荷受人マスタの[登録ＰＧ]に出荷使用日を更新 START
//// MOD 2009.01.30 東都）高木 [名前３]に最終利用年月を更新 START
////					+         "'" + sData[9] +"','" + sData[10] +"','" + sData[11] +"',' ','" + sData[5] + sData[6] +"','" + s住所ＣＤ + "', \n"
//					+         "'" + sData[9] +"','" + sData[10] +"','" + sData[11] +"',TO_CHAR(SYSDATE,'YYYYMM'),'" + sData[5] + sData[6] +"','" + s住所ＣＤ + "', \n"
//// MOD 2009.01.30 東都）高木 [名前３]に最終利用年月を更新 END
					+         "'" + sData[9] +"','" + sData[10] +"','" + sData[11] +"',' ','" + sData[5] + sData[6] +"','" + s住所ＣＤ + "', \n"
// MOD 2010.02.02 東都）高木 荷受人マスタの[登録ＰＧ]に出荷使用日を更新 END
// MOD 2006.07.10 東都）高木 住所ＣＤの追加 END
// MOD 2008.06.11 kcl)森本 着店コード検索方法の変更 START
//					+         "'" + sData[1] +"',' ',' ','" + sData[12] +"','" + sData[13] +"', \n"
					+         "'" + sData[1] +"',' ','" + s特殊ＣＤ + "','" + sData[12] +"','" + sData[13] +"', \n"
// MOD 2008.06.11 kcl)森本 着店コード検索方法の変更 END
// MOD 2010.02.02 東都）高木 荷受人マスタの[登録ＰＧ]に出荷使用日を更新 START
//					+         "'0',TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'),'" + sData[14] +"','" + sData[15] +"', \n"
					+         "'0',TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'),' ','" + sData[15] +"', \n"
// MOD 2010.02.02 東都）高木 荷受人マスタの[登録ＰＧ]に出荷使用日を更新 END
					+         "TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'),'" + sData[14] +"','" + sData[15] +"')";

				iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);
				tran.Commit();
				sRet[0] = "正常終了";
				
			}
			catch (OracleException ex)
			{
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				tran.Rollback();
// DEL 2005.05.31 東都）高木 不要な為削除 START
//				string sErr = ex.Message.Substring(0,9);
//				if(sErr == "ORA-00001")
//					sRet[0] = "同一のコードが既に他の端末より登録されています。\r\n再度、最新データを呼び出して更新してください。";
//				else
// DEL 2005.05.31 東都）高木 不要な為削除 END
				sRet[0] = "サーバエラー：" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				conn2 = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
			}
			
			return sRet;
		}

// MOD 2010.06.03 東都）高木 複数件削除機能の高速化 START
		/*********************************************************************
		 * 届け先データ削除
		 * 引数：会員ＣＤ、部門ＣＤ、荷受人ＣＤ、更新ＰＧ、更新者
		 * 戻値：ステータス
		 *
		 *********************************************************************/
		[WebMethod]
		public String[] Del_todokesakis(string[] sUser, string[] sData, string[] sList)
		{
			logWriter(sUser, INF, "届け先複数件削除開始");

			OracleConnection conn2 = null;
			string[] sRet = new string[sList.Length + 1];
			sRet[0] = "";
			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null){
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			try{
				string cmdQuery; 
				for(int iCnt = 0; iCnt < sList.Length; iCnt++){
					sRet[iCnt + 1] = "";
					if(sList[iCnt] == null) continue;
					if(sList[iCnt].Length == 0) continue;
					cmdQuery 
						= "UPDATE ＳＭ０２荷受人 \n"
						+    "SET 削除ＦＧ           = '1', \n"
						+        "更新ＰＧ           = '" + sData[3] +"', \n"
						+        "更新者             = '" + sData[4] +"', \n"
						+        "更新日時           =  TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') \n"
						+ " WHERE 会員ＣＤ           = '" + sData[0] +"' \n"
						+ "   AND 部門ＣＤ           = '" + sData[1] +"' \n"
						+ "   AND 荷受人ＣＤ         = '" + sList[iCnt] +"'";
					int iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);
					sRet[iCnt + 1] = iUpdRow.ToString();
				}

				tran.Commit();				
				sRet[0] = "正常終了";
			}catch (OracleException ex){
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}catch (Exception ex){
				tran.Rollback();
				sRet[0] = "サーバエラー：" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}finally{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			
			return sRet;
		}

// MOD 2010.06.03 東都）高木 複数件削除機能の高速化 END
		/*********************************************************************
		 * 届け先データ削除
		 * 引数：会員ＣＤ、部門ＣＤ、荷受人ＣＤ、更新ＰＧ、更新者
		 * 戻値：ステータス
		 *
		 *********************************************************************/
		[WebMethod]
		public String[] Del_todokesaki(string[] sUser, string[] sData)
		{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//			logFileOpen(sUser);
			logWriter(sUser, INF, "届け先削除開始");

			OracleConnection conn2 = null;
			string[] sRet = new string[5];
			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//// ADD 2005.05.23 東都）伊賀 会員チェック追加 START
//			// 会員チェック
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//// ADD 2005.05.23 東都）伊賀 会員チェック追加 END

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			try
			{
				string cmdQuery 
					= "UPDATE ＳＭ０２荷受人 \n"
					+    "SET 削除ＦＧ           = '1', \n"
					+        "更新ＰＧ           = '" + sData[3] +"', \n"
					+        "更新者             = '" + sData[4] +"', \n"
					+        "更新日時           =  TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') \n"
					+ " WHERE 会員ＣＤ           = '" + sData[0] +"' \n"
					+ "   AND 部門ＣＤ           = '" + sData[1] +"' \n"
					+ "   AND 荷受人ＣＤ         = '" + sData[2] +"'";

				int iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);
				tran.Commit();				
				sRet[0] = "正常終了";
				
			}
			catch (OracleException ex)
			{
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				tran.Rollback();
				sRet[0] = "サーバエラー：" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				conn2 = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
			}
			
			return sRet;
		}

		// ADD Hijiya Start  届け先削除２
		[WebMethod]
		public String[] Del_todokesaki2(string[] sUser, string sKCode, string sBCode, string sTCode)
		{
			OracleConnection conn2 = null;
			string[] sRet = new string[5];
			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//// ADD 2005.05.23 東都）伊賀 会員チェック追加 START
//			// 会員チェック
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//// ADD 2005.05.23 東都）伊賀 会員チェック追加 END

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			try
			{
				string cmdQuery 
					= "DELETE FROM ＳＭ０２荷受人 "
					+ " WHERE 会員ＣＤ           = '" + sKCode +"'"
					+ "   AND 部門ＣＤ           = '" + sBCode +"'"
					+ "   AND 荷受人ＣＤ         = '" + sTCode +"'"
					+ "   AND 削除ＦＧ           = '1'";

// MOD 2005.06.08 東都）高木 ＤＢ接続方法の変更 START
//				OracleCommand cmd = new OracleCommand(cmdQuery);
//				cmd.Connection = conn;
//				cmd.CommandType = CommandType.Text;
//
//				cmd.ExecuteNonQuery();
				int iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);
// MOD 2005.06.08 東都）高木 ＤＢ接続方法の変更 END
				tran.Commit();				
				sRet[0] = "1";
				
			}
// ADD 2005.05.27 東都）高木 メッセージの変更 START
			catch (OracleException ex)
			{
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
// ADD 2005.05.27 東都）高木 メッセージの変更 END
			catch (Exception ex)
			{
				tran.Rollback();
// MOD 2005.05.27 東都）高木 メッセージの変更 START
//				sRet[0] = "ＤＢエラー：" + ex.Message;
				sRet[0] = "サーバエラー：" + ex.Message;
// MOD 2005.05.27 東都）高木 メッセージの変更 END
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				conn2 = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
			}

			return sRet;
		}
		// ADD Hijiya End

// ADD 2006.12.14 東都）小童谷 お届先情報全件削除 START
		/*********************************************************************
		 * 届け先データ削除３
		 * 引数：会員ＣＤ、部門ＣＤ、更新ＰＧ、更新者
		 * 戻値：ステータス
		 *
		 *********************************************************************/
		[WebMethod]
		public String[] Del_todokesaki3(string[] sUser, string[] sData)
		{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//			logFileOpen(sUser);
			logWriter(sUser, INF, "届け先全件削除開始");

			OracleConnection conn2 = null;
			string[] sRet = new string[5];
			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//
//			// 会員チェック
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//
			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			try
			{
				string cmdQuery 
					= "UPDATE ＳＭ０２荷受人 \n"
					+    "SET 削除ＦＧ           = '1', \n"
					+        "更新ＰＧ           = '" + sData[2] +"', \n"
					+        "更新者             = '" + sData[3] +"', \n"
					+        "更新日時           =  TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') \n"
					+ " WHERE 会員ＣＤ           = '" + sData[0] +"' \n"
					+ "   AND 部門ＣＤ           = '" + sData[1] +"'"
					+ "   AND 削除ＦＧ           = '0'";

				int iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);
				tran.Commit();				
				sRet[0] = "正常終了";
				
			}
			catch (OracleException ex)
			{
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				tran.Rollback();
				sRet[0] = "サーバエラー：" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				conn2 = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
			}
			
			return sRet;
		}
// ADD 2006.12.14 東都）小童谷 お届先情報全件削除 END

		/*********************************************************************
		 * アップロードデータ追加
		 * 引数：会員ＣＤ、部門ＣＤ、荷受人ＣＤ...
		 * 戻値：ステータス
		 *
		 *********************************************************************/
		[WebMethod]
		public String[] Ins_uploadData(string[] sUser, string[] sList)
		{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//			logFileOpen(sUser);
			logWriter(sUser, INF, "アップロードデータ追加開始");

			OracleConnection conn2 = null;
			string[] sRet = new string[1];
//			string s更新日時 = System.DateTime.Now.ToString("yyyyMMddHHmmss");

			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//// ADD 2005.05.23 東都）伊賀 会員チェック追加 START
//			// 会員チェック
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//// ADD 2005.05.23 東都）伊賀 会員チェック追加 END

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			sRet[0] = "";
			try
			{
				for (int i = 0; i < sList.Length; i++)
				{
					string[] sData = sList[i].Split(',');
// ADD 2006.07.10 東都）高木 住所ＣＤの追加 START
					string s住所ＣＤ = " ";
					if(sData.Length >= 22 && sData[21].Length > 0){
						s住所ＣＤ = sData[21];
					}
// ADD 2006.07.10 東都）高木 住所ＣＤの追加 END
// ADD 2008.06.11 kcl)森本 着店コード検索方法の変更 START
					string s特殊ＣＤ = " ";
//					if (sData.Length >=20 && sData[19].Length > 0) 
//					{
//						s特殊ＣＤ = sData[19];
//					}
// ADD 2008.06.11 kcl)森本 着店コード検索方法の変更 END

					string cmdQuery = "";
					cmdQuery
						= "SELECT 削除ＦＧ \n"
						+   "FROM ＳＭ０２荷受人 \n"
						+  "WHERE 会員ＣＤ = '" + sData[0] + "' \n"
						+    "AND 部門ＣＤ = '" + sData[1] + "' \n"
						+    "AND 荷受人ＣＤ = '" + sData[2] + "' "
						+    "FOR UPDATE ";

					OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
					int iCnt = 1;
					string s削除ＦＧ = "";
					while (reader.Read())
					{
						s削除ＦＧ = reader.GetString(0);
						iCnt++;
					}
// ADD 2005.06.08 東都）伊賀 ORA-01000対策 START
					reader.Close();
// ADD 2005.06.08 東都）伊賀 ORA-01000対策 END
					if(iCnt == 1)
					{
						//追加
						cmdQuery 
							= "INSERT INTO ＳＭ０２荷受人 \n"
							+ "VALUES ( "
							+           "'" + sData[0] + "', "
							+           "'" + sData[1] + "', \n"
							+           "'" + sData[2] + "', "
							+           "'" + sData[3] + "', \n"
							+           "'" + sData[4] + "', "
							+           "'" + sData[5] + "', \n"
							+           "'" + sData[6] + "', "
							+           "'" + sData[7] + "', \n"
							+           "'" + sData[8] + "', "
							+           "'" + sData[9] + "', \n"
							+           "'" + sData[10] + "', "
							+           "'" + sData[11] + "', \n"
							+           "'" + sData[12] + "', "
							+           "'" + sData[13] + "', \n"
// MOD 2010.02.02 東都）高木 荷受人マスタの[登録ＰＧ]に出荷使用日を更新 START
//// MOD 2009.01.30 東都）高木 [名前３]に最終利用年月を更新 START
////							+           "'" + sData[14] + "', "
//							+           "TO_CHAR(SYSDATE,'YYYYMM'), "
//// MOD 2009.01.30 東都）高木 [名前３]に最終利用年月を更新 END
							+           "'" + sData[14] + "', "
// MOD 2010.02.02 東都）高木 荷受人マスタの[登録ＰＧ]に出荷使用日を更新 END
							+           "'" + sData[15] + "', \n"
// MOD 2006.07.10 東都）高木 住所ＣＤの追加 START
//							+           "' ', "
							+           "'" + s住所ＣＤ + "', "
// MOD 2006.07.10 東都）高木 住所ＣＤの追加 END
							+           "'" + sData[16] + "', \n"
							+           "'" + sData[17] + "', "
// ADD 2008.06.11 kcl)森本 着店コード検索方法の変更 START
//							+           "' ', "
							+           "'" + s特殊ＣＤ + "', \n"
// ADD 2008.06.11 kcl)森本 着店コード検索方法の変更 END
							+           "'" + sData[18] + "', \n"
							+           "' ', "
							+           "'0', "
							+           "TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), \n"
// MOD 2010.02.02 東都）高木 荷受人マスタの[登録ＰＧ]に出荷使用日を更新 START
//							+           "'お届取込', "
							+           "' ', \n"
// MOD 2010.02.02 東都）高木 荷受人マスタの[登録ＰＧ]に出荷使用日を更新 END
							+           "'" + sData[20] + "', \n"
							+           "TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), "
							+           "'お届取込', \n"
							+           "'" + sData[20] + "')";

						CmdUpdate(sUser, conn2, cmdQuery);

//保留
//						tran.Commit();
					}
					else
					{
						//上書き更新
						cmdQuery
							= "UPDATE ＳＭ０２荷受人 \n"
							+    "SET 電話番号１ = '" + sData[3] + "' "
							+       ",電話番号２ = '" + sData[4] + "' \n"
							+       ",電話番号３ = '" + sData[5] + "' "
							+       ",ＦＡＸ番号１ = '" + sData[6] + "' \n"
							+       ",ＦＡＸ番号２ = '" + sData[7] + "' "
							+       ",ＦＡＸ番号３ = '" + sData[8] + "' \n"
							+       ",住所１ = '" + sData[9] + "' "
							+       ",住所２ = '" + sData[10] + "' \n"
							+       ",住所３ = '" + sData[11] + "' "
							+       ",名前１ = '" + sData[12] + "' \n"
							+       ",名前２ = '" + sData[13] + "' "
// MOD 2010.02.02 東都）高木 荷受人マスタの[登録ＰＧ]に出荷使用日を更新 START
//// MOD 2009.01.30 東都）高木 [名前３]に最終利用年月を更新 START
////							+       ",名前３ = '" + sData[14] + "' \n"
//							+       ",名前３ = TO_CHAR(SYSDATE,'YYYYMM') \n"
//// MOD 2009.01.30 東都）高木 [名前３]に最終利用年月を更新 END
							+       ",名前３ = '" + sData[14] + "' \n"
// MOD 2010.02.02 東都）高木 荷受人マスタの[登録ＰＧ]に出荷使用日を更新 END
							+       ",郵便番号 = '" + sData[15] + "' "
// MOD 2006.07.10 東都）高木 住所ＣＤの追加 START
//							+       ",住所ＣＤ = ' ' \n"
							+       ",住所ＣＤ = '" + s住所ＣＤ + "' \n"
// MOD 2006.07.10 東都）高木 住所ＣＤの追加 END
							+       ",カナ略称 = '" + sData[16] + "' "
							+       ",一斉出荷区分 = '" + sData[17] + "' \n"
// ADD 2008.06.13 kcl)森本 着店コード検索方法の変更 START
//							+       ",特殊ＣＤ = ' ' "
							+       ",特殊ＣＤ = '" + s特殊ＣＤ + "' \n"
// ADD 2008.06.13 kcl)森本 着店コード検索方法の変更 END
							+       ",特殊計 = '" + sData[18] + "' \n"
							+       ",メールアドレス = ' ' "
// MOD 2005.06.29 東都）小童谷 削除ＦＧが０の時は登録日等は更新しない START
//							+       ",削除ＦＧ = '0' \n"
//							+       ",登録日時 = " + s更新日時
//							+       ",登録ＰＧ = 'お届取込' "
//							+       ",登録者 = '" + sData[20] + "' \n"
//							+       ",更新日時 = " + s更新日時
//							+       ",更新ＰＧ = 'お届取込' "
//							+       ",更新者 = '" + sData[20] + "' \n"
//							+ "WHERE 会員ＣＤ = '" + sData[0] + "' \n"
//							+   "AND 部門ＣＤ = '" + sData[1] + "' \n"
//							+   "AND 荷受人ＣＤ = '" + sData[2] + "' ";
							+       ",削除ＦＧ = '0' \n"
// MOD 2010.02.02 東都）高木 荷受人マスタの[登録ＰＧ]に出荷使用日を更新 START
							+       ",登録ＰＧ = ' ' \n"
// MOD 2010.02.02 東都）高木 荷受人マスタの[登録ＰＧ]に出荷使用日を更新 END
							;
						if(s削除ＦＧ == "1")
						{
							cmdQuery
								+=  ",登録日時 = TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') "
// MOD 2010.02.02 東都）高木 荷受人マスタの[登録ＰＧ]に出荷使用日を更新 START
//								+   ",登録ＰＧ = 'お届取込' "
// MOD 2010.02.02 東都）高木 荷受人マスタの[登録ＰＧ]に出荷使用日を更新 END
								+   ",登録者 = '" + sData[20] + "' \n";
						}
						cmdQuery
							+=      ",更新日時 = TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') "
							+       ",更新ＰＧ = 'お届取込' "
							+       ",更新者 = '" + sData[20] + "' \n"
							+ "WHERE 会員ＣＤ = '" + sData[0] + "' \n"
							+   "AND 部門ＣＤ = '" + sData[1] + "' \n"
							+   "AND 荷受人ＣＤ = '" + sData[2] + "' ";
// MOD 2005.06.29 東都）小童谷 削除ＦＧが０の時は登録日等は更新しない END

							CmdUpdate(sUser, conn2, cmdQuery);
					}
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
					disposeReader(reader);
					reader = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END
				}
				logWriter(sUser, INF, "正常終了");
				tran.Commit();
			}
			catch (OracleException ex)
			{
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				tran.Rollback();
				sRet[0] = "サーバエラー：" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				conn2 = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
			}
			return sRet;
		}

// MOD 2010.03.11 東都）高木 ＣＳＶ取込時の郵便番号１００件一括チェックを追加 START
		/*********************************************************************
		 * アップロードデータ追加２
		 * 引数：会員ＣＤ、部門ＣＤ、荷受人ＣＤ...
		 * 戻値：ステータス
		 *
		 *********************************************************************/
		private static string INS_UPLOADDATA2_SELECT1
			= "SELECT 1 \n"
			+ " FROM ＣＭ１４郵便番号 \n"
			;

		[WebMethod]
		public String[] Ins_uploadData2(string[] sUser, string[] sList)
		{
			logWriter(sUser, INF, "アップロードデータ追加２開始");

			OracleConnection conn2 = null;
			string[] sRet = new string[sList.Length + 1];

			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null){
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			sRet[0] = "";
			try{
				for (int iRow = 0; iRow < sList.Length; iRow++){
					sRet[iRow+1] = "";

					string[] sData = sList[iRow].Split(',');
					string s住所ＣＤ = " ";
					if(sData.Length >= 22 && sData[21].Length > 0){
						s住所ＣＤ = sData[21];
					}
// ADD 2008.06.11 kcl)森本 着店コード検索方法の変更 START
					string s特殊ＣＤ = " ";
//					if (sData.Length >=20 && sData[19].Length > 0) 
//					{
//						s特殊ＣＤ = sData[19];
//					}
// ADD 2008.06.11 kcl)森本 着店コード検索方法の変更 END

//					sData[15] = sData[15].TrimEnd();
//					if(sData[15].Length == 0){
//						sRet[iRow+1] = "郵未";//未設定
//						continue;
//					}
//					if(sData[15].Length != 7){
//						sRet[iRow+1] = "郵桁";//桁数に誤りがある場合
//						continue;
//					}

					//郵便番号マスタの存在チェック
					OracleDataReader reader;
					string cmdQuery = "";
					cmdQuery = INS_UPLOADDATA2_SELECT1
							+ "WHERE 郵便番号 = '" + sData[15] + "' \n"
//保留 MOD 2010.04.13 東都）高木 郵便番号が削除された時の障害対応 START
							+ "AND 削除ＦＧ = '0' \n"
//保留 MOD 2010.04.13 東都）高木 郵便番号が削除された時の障害対応 END
							;

					reader = CmdSelect(sUser, conn2, cmdQuery);
					if(!reader.Read()){
						sRet[iRow+1] = sData[15];//該当データ無し
						reader.Close();
						disposeReader(reader);
						reader = null;
						continue;
					}
					reader.Close();

					cmdQuery
						= "SELECT 削除ＦＧ \n"
						+   "FROM ＳＭ０２荷受人 \n"
						+  "WHERE 会員ＣＤ = '" + sData[0] + "' \n"
						+    "AND 部門ＣＤ = '" + sData[1] + "' \n"
						+    "AND 荷受人ＣＤ = '" + sData[2] + "' "
						+    "FOR UPDATE "
						;

					reader = CmdSelect(sUser, conn2, cmdQuery);
					int iCnt = 1;
					string s削除ＦＧ = "";
					while (reader.Read()){
						s削除ＦＧ = reader.GetString(0);
						iCnt++;
					}
					reader.Close();

					if(iCnt == 1){
						//追加
						cmdQuery 
							= "INSERT INTO ＳＭ０２荷受人 \n"
							+ "VALUES ( "
							+           "'" + sData[0] + "', "
							+           "'" + sData[1] + "', \n"
							+           "'" + sData[2] + "', "
							+           "'" + sData[3] + "', \n"
							+           "'" + sData[4] + "', "
							+           "'" + sData[5] + "', \n"
							+           "'" + sData[6] + "', "
							+           "'" + sData[7] + "', \n"
							+           "'" + sData[8] + "', "
							+           "'" + sData[9] + "', \n"
							+           "'" + sData[10] + "', "
							+           "'" + sData[11] + "', \n"
							+           "'" + sData[12] + "', "
							+           "'" + sData[13] + "', \n"
							+           "'" + sData[14] + "', "
							+           "'" + sData[15] + "', \n"
							+           "'" + s住所ＣＤ + "', "
							+           "'" + sData[16] + "', \n"
							+           "'" + sData[17] + "', "
// ADD 2008.06.11 kcl)森本 着店コード検索方法の変更 START
//							+           "' ', \n" //特殊ＣＤ
							+           "'" + s特殊ＣＤ + "', \n"
// ADD 2008.06.11 kcl)森本 着店コード検索方法の変更 END
							+           "'" + sData[18] + "', \n"
							+           "' ', "
							+           "'0', "
							+           "TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), \n"
							+           "' ', \n"
							+           "'" + sData[20] + "', \n"
							+           "TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), "
							+           "'お届取込', \n"
							+           "'" + sData[20] + "')"
							;
						CmdUpdate(sUser, conn2, cmdQuery);
					}else{
						//上書き更新
						cmdQuery
							= "UPDATE ＳＭ０２荷受人 \n"
							+    "SET 電話番号１ = '" + sData[3] + "' "
							+       ",電話番号２ = '" + sData[4] + "' \n"
							+       ",電話番号３ = '" + sData[5] + "' "
							+       ",ＦＡＸ番号１ = '" + sData[6] + "' \n"
							+       ",ＦＡＸ番号２ = '" + sData[7] + "' "
							+       ",ＦＡＸ番号３ = '" + sData[8] + "' \n"
							+       ",住所１ = '" + sData[9] + "' "
							+       ",住所２ = '" + sData[10] + "' \n"
							+       ",住所３ = '" + sData[11] + "' "
							+       ",名前１ = '" + sData[12] + "' \n"
							+       ",名前２ = '" + sData[13] + "' "
							+       ",名前３ = '" + sData[14] + "' \n"
							+       ",郵便番号 = '" + sData[15] + "' "
							+       ",住所ＣＤ = '" + s住所ＣＤ + "' \n"
							+       ",カナ略称 = '" + sData[16] + "' "
							+       ",一斉出荷区分 = '" + sData[17] + "' \n"
// ADD 2008.06.13 kcl)森本 着店コード検索方法の変更 START
//							+       ",特殊ＣＤ = ' ' \n" //特殊ＣＤ
							+       ",特殊ＣＤ = '" + s特殊ＣＤ + "' \n"
// ADD 2008.06.13 kcl)森本 着店コード検索方法の変更 END
							+       ",特殊計 = '" + sData[18] + "' \n"
							+       ",メールアドレス = ' ' "
							+       ",削除ＦＧ = '0' \n"
							+       ",登録ＰＧ = ' ' \n"
							;
						if(s削除ＦＧ == "1"){
							cmdQuery
								+=  ",登録日時 = TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') "
								+   ",登録者 = '" + sData[20] + "' \n"
								;
						}
						cmdQuery
							+=      ",更新日時 = TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') "
							+       ",更新ＰＧ = 'お届取込' "
							+       ",更新者 = '" + sData[20] + "' \n"
							+ "WHERE 会員ＣＤ = '" + sData[0] + "' \n"
							+   "AND 部門ＣＤ = '" + sData[1] + "' \n"
							+   "AND 荷受人ＣＤ = '" + sData[2] + "' "
							;

							CmdUpdate(sUser, conn2, cmdQuery);
					}
					disposeReader(reader);
					reader = null;
				}
				logWriter(sUser, INF, "正常終了");
				tran.Commit();
			}catch (OracleException ex){
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}catch (Exception ex){
				tran.Rollback();
				sRet[0] = "サーバエラー：" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}finally{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			return sRet;
		}
// MOD 2010.03.11 東都）高木 ＣＳＶ取込時の郵便番号１００件一括チェックを追加 END

// ADD 2005.06.08 東都）伊賀 ＣＳＶ出力追加 START
		[WebMethod]
		public String[] Get_csvwrite(string[] sUser, string sKCode, string sBCode)
		{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//			logFileOpen(sUser);
			logWriter(sUser, INF, "ＣＳＶ出力用取得開始");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();

			string[] sRet = new string[1];
			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//
//			// 会員チェック
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//
			StringBuilder sbQuery = new StringBuilder(1024);
			try
			{
				sbQuery.Append("SELECT 荷受人ＣＤ \n");
				sbQuery.Append(      ",電話番号１ \n");
				sbQuery.Append(      ",電話番号２ \n");
				sbQuery.Append(      ",電話番号３ \n");
				sbQuery.Append(      ",ＦＡＸ番号１ \n");
				sbQuery.Append(      ",ＦＡＸ番号２ \n");
				sbQuery.Append(      ",ＦＡＸ番号３ \n");
				sbQuery.Append(      ",住所１ \n");
				sbQuery.Append(      ",住所２ \n");
				sbQuery.Append(      ",住所３ \n");
				sbQuery.Append(      ",名前１ \n");
				sbQuery.Append(      ",名前２ \n");
				sbQuery.Append(      ",' ' \n");
				sbQuery.Append(      ",郵便番号 \n");
				sbQuery.Append(      ",カナ略称 \n");
				sbQuery.Append(      ",一斉出荷区分 \n");
				sbQuery.Append(      ",特殊計 \n");
// MOD 2008.06.11 kcl)森本 着店コード検索方法の変更 START
//				sbQuery.Append(      ",' ' \n");
				sbQuery.Append(      ",特殊ＣＤ \n");
// MOD 2008.06.11 kcl)森本 着店コード検索方法の変更 END
				sbQuery.Append( "FROM ＳＭ０２荷受人 \n");
				sbQuery.Append("WHERE 会員ＣＤ = '" + sKCode + "' \n");
				sbQuery.Append(  "AND 部門ＣＤ = '" + sBCode + "' \n");
				sbQuery.Append(  "AND 削除ＦＧ = '0' \n");
// ADD 2009.01.29 東都）高木 一覧のソート順に[荷受人ＣＤ]を追加 START
				sbQuery.Append("ORDER BY 荷受人ＣＤ \n");
// ADD 2009.01.29 東都）高木 一覧のソート順に[荷受人ＣＤ]を追加 END

				OracleDataReader reader;
				reader = CmdSelect(sUser, conn2, sbQuery);

				StringBuilder sbData;
				while (reader.Read())
				{
					sbData = new StringBuilder(1024);
// MOD 2010.02.03 東都）高木 ＣＳＶ出力時に文字項目の前に[']を追加 START
//					sbData.Append(sDbl + reader.GetString(0).Trim() + sDbl);			// 荷受人ＣＤ
//					sbData.Append(sKanma + sDbl + "(" + reader.GetString(1).Trim());	// 電話番号１
//					sbData.Append(")" + reader.GetString(2).Trim() + "-");				// 電話番号２
//					sbData.Append(reader.GetString(3).Trim() + sDbl);					// 電話番号３
//// MOD 2005.07.04 東都）高木 ＦＡＸ番号の形式エラー START
////					sbData.Append(sKanma + sDbl + reader.GetString(4).Trim());			// ＦＡＸ番号１
////					sbData.Append(reader.GetString(5).Trim());							// ＦＡＸ番号２
////					sbData.Append(reader.GetString(6).Trim() + sDbl);					// ＦＡＸ番号３
//					sbData.Append(sKanma + sDbl);
//					if(reader.GetString(4).Trim().Length > 0) 
//						sbData.Append("(" + reader.GetString(4).Trim() + ")");			// ＦＡＸ番号１
//					if(reader.GetString(5).Trim().Length > 0) 
//						sbData.Append(reader.GetString(5).Trim() + "-");				// ＦＡＸ番号２
//					sbData.Append(reader.GetString(6).Trim() + sDbl);					// ＦＡＸ番号３
//// MOD 2005.07.04 東都）高木 ＦＡＸ番号の形式エラー END
//					sbData.Append(sKanma + sDbl + reader.GetString(7).Trim() + sDbl);	// 住所１
//					sbData.Append(sKanma + sDbl + reader.GetString(8).Trim() + sDbl);	// 住所２
//					sbData.Append(sKanma + sDbl + reader.GetString(9).Trim() + sDbl);	// 住所３
//					sbData.Append(sKanma + sDbl + reader.GetString(10).Trim() + sDbl);	// 名前１
//					sbData.Append(sKanma + sDbl + reader.GetString(11).Trim() + sDbl);	// 名前２
//					sbData.Append(sKanma + sDbl + reader.GetString(12).Trim() + sDbl);	// 名前３
//					sbData.Append(sKanma + sDbl + reader.GetString(13).Trim() + sDbl);	// 郵便番号
//					sbData.Append(sKanma + sDbl + reader.GetString(14).Trim() + sDbl);	// カナ略称
//					sbData.Append(sKanma + sDbl + reader.GetString(15).Trim() + sDbl);	// 一斉出荷区分
//					sbData.Append(sKanma + sDbl + reader.GetString(16).Trim() + sDbl);	// 特殊計
//					sbData.Append(sKanma + sDbl + reader.GetString(17).Trim() + sDbl);	// 着店ＣＤ
					sbData.Append(sDbl + sSng + reader.GetString(0).TrimEnd() + sDbl);	// 荷受人ＣＤ
					sbData.Append(sKanma + sDbl + sSng);
					if(reader.GetString(1).TrimEnd().Length > 0) 
						sbData.Append("(" + reader.GetString(1).TrimEnd() + ")");		// 電話番号１
					if(reader.GetString(2).TrimEnd().Length > 0) 
						sbData.Append(reader.GetString(2).TrimEnd() + "-");				// 電話番号２
					sbData.Append(reader.GetString(3).TrimEnd() + sDbl);				// 電話番号３
					sbData.Append(sKanma + sDbl + sSng);
					if(reader.GetString(4).TrimEnd().Length > 0) 
						sbData.Append("(" + reader.GetString(4).TrimEnd() + ")");		// ＦＡＸ番号１
					if(reader.GetString(5).TrimEnd().Length > 0) 
						sbData.Append(reader.GetString(5).TrimEnd() + "-");				// ＦＡＸ番号２
					sbData.Append(reader.GetString(6).TrimEnd() + sDbl);				// ＦＡＸ番号３
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(7).TrimEnd() + sDbl);	// 住所１
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(8).TrimEnd() + sDbl);	// 住所２
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(9).TrimEnd() + sDbl);	// 住所３
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(10).TrimEnd() + sDbl);// 名前１
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(11).TrimEnd() + sDbl);// 名前２
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(12).TrimEnd() + sDbl);// 名前３
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(13).TrimEnd() + sDbl);// 郵便番号
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(14).TrimEnd() + sDbl);// カナ略称
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(15).TrimEnd() + sDbl);// 一斉出荷区分
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(16).TrimEnd() + sDbl);// 特殊計
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(17).TrimEnd() + sDbl);// 着店ＣＤ
// MOD 2010.02.03 東都）高木 ＣＳＶ出力時に文字項目の前に[']を追加 START
					sList.Add(sbData);
				}
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				disposeReader(reader);
				reader = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END

				sRet = new string[sList.Count + 1];
				if(sList.Count == 0) 
					sRet[0] = "該当データがありません";
				else
				{
					sRet[0] = "正常終了";
					int iCnt = 1;
					IEnumerator enumList = sList.GetEnumerator();
					while(enumList.MoveNext())
					{
						sRet[iCnt] = enumList.Current.ToString();
						iCnt++;
					}
				}
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "サーバエラー：" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				conn2 = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
			}
			return sRet;
		}
// ADD 2005.06.08 東都）伊賀 ＣＳＶ出力追加 END

// ADD 2006.07.03 東都）山本 荷主総件数表示 START
		[WebMethod]
		public String[] Get_ninushiCount(string[] sUser, string sKCode, string sBCode)
		{
// MOD 2010.05.10 東都）高木 ＰＧ障害の修正 START
			return Get_otodokeCount(sUser, sKCode, sBCode);
		}

		[WebMethod]
		public String[] Get_otodokeCount(string[] sUser, string sKCode, string sBCode)
		{
// MOD 2010.05.10 東都）高木 ＰＧ障害の修正 END
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//			logFileOpen(sUser);
// MOD 2010.05.10 東都）高木 ＰＧ障害の修正 START
//			logWriter(sUser, INF, "荷主総件数取得開始");
			logWriter(sUser, INF, "お届け先総件数取得開始");
// MOD 2010.05.10 東都）高木 ＰＧ障害の修正 END

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[2];
			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//
//			// 会員チェック
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//
			StringBuilder sbQuery = new StringBuilder(1024);
			StringBuilder sbRet = new StringBuilder(1024);
			try
			{
				sbQuery.Append("SELECT COUNT(*) \n");
				sbQuery.Append("FROM ＳＭ０２荷受人  \n");
				sbQuery.Append(" WHERE 会員ＣＤ = '" + sKCode + "' \n");
				sbQuery.Append("   AND 部門ＣＤ = '" + sBCode + "' \n");
				sbQuery.Append(" AND 削除ＦＧ = '0' \n");

				OracleDataReader reader = CmdSelect(sUser, conn2, sbQuery);
// MOD 2010.05.10 東都）高木 ＰＧ障害の修正 START
//				reader.Read();
//				if (reader.GetDecimal(0) == 0)
//				{
//					sRet[0] = "指定された荷主は存在しません";
//					sRet[1] = "0";
//				}
//				else
//				{
//					sRet[0] = "正常終了";
//					sRet[1] = reader.GetDecimal(0).ToString().Trim();
//				}
				if(reader.Read()){
					sRet[0] = "正常終了";
					sRet[1] = reader.GetDecimal(0).ToString().Trim();
				}else{
					sRet[0] = "お届け先マスタ読み込み時にエラーが発生しました";
					sRet[1] = "0";
				}
// MOD 2010.05.10 東都）高木 ＰＧ障害の修正 END
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				disposeReader(reader);
				reader = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "サーバエラー：" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				conn2 = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
			}
			
			return sRet;
		}
// ADD 2006.07.03 東都）山本 荷主総件数表示 END

// ADD 2006.07.05 東都）山本 アドレス帳画面からのＣＳＶ出力対応 START
// ADD 2007.02.14 FJCS）桑田 検索条件に名前の追加 START
		[WebMethod]
		public String[] Get_csvwrite2(string[] sUser, string sKCode, string sBCode, string sKana, string sTCode, string sTelNo, string sTelNo2, string sTelNo3,string sName,int iSortLabel1,int iSortPat1,int iSortLabel2,int iSortPat2)
// ADD 2007.02.14 FJCS）桑田 検索条件に名前の追加 END
// MOD 2010.02.03 東都）高木 検索条件に更新日を追加 START
		{
			return Get_csvwrite3(sUser, sKCode, sBCode, sKana, sTCode
								, sTelNo, sTelNo2, sTelNo3, sName
								, iSortLabel1, iSortPat1, iSortLabel2, iSortPat2
								, ""
								);
		}

		[WebMethod]
		public String[] Get_csvwrite3(string[] sUser, string sKCode, string sBCode, string sKana, string sTCode, string sTelNo, string sTelNo2, string sTelNo3,string sName,int iSortLabel1,int iSortPat1,int iSortLabel2,int iSortPat2, string sUpdateDay)
// MOD 2010.02.03 東都）高木 検索条件に更新日を追加 END
		{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//			logFileOpen(sUser);
			logWriter(sUser, INF, "ＣＳＶ出力用取得開始３");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();

			string[] sRet = new string[1];
			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//
//			// 会員チェック
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//
			StringBuilder sbQuery = new StringBuilder(1024);
			try
			{
				sbQuery.Append("SELECT 荷受人ＣＤ \n");
				sbQuery.Append(      ",電話番号１ \n");
				sbQuery.Append(      ",電話番号２ \n");
				sbQuery.Append(      ",電話番号３ \n");
				sbQuery.Append(      ",ＦＡＸ番号１ \n");
				sbQuery.Append(      ",ＦＡＸ番号２ \n");
				sbQuery.Append(      ",ＦＡＸ番号３ \n");
				sbQuery.Append(      ",住所１ \n");
				sbQuery.Append(      ",住所２ \n");
				sbQuery.Append(      ",住所３ \n");
				sbQuery.Append(      ",名前１ \n");
				sbQuery.Append(      ",名前２ \n");
				sbQuery.Append(      ",' ' \n");
				sbQuery.Append(      ",郵便番号 \n");
				sbQuery.Append(      ",カナ略称 \n");
				sbQuery.Append(      ",一斉出荷区分 \n");
				sbQuery.Append(      ",特殊計 \n");
// MOD 2008.06.11 kcl)森本 着店コード検索方法の変更 START
//				sbQuery.Append(      ",' ' \n");
				sbQuery.Append(      ",特殊ＣＤ \n");
// MOD 2008.06.11 kcl)森本 着店コード検索方法の変更 END
				sbQuery.Append( "FROM ＳＭ０２荷受人 \n");
				sbQuery.Append("WHERE 会員ＣＤ = '" + sKCode + "' \n");
				sbQuery.Append(  "AND 部門ＣＤ = '" + sBCode + "' \n");
				if(sKana.Length > 0 && sTCode.Length == 0)
				{
					sbQuery.Append(" AND カナ略称 LIKE '%"+ sKana + "%' \n");
				}
				if(sTCode.Length > 0 && sKana.Length == 0)
				{
					sbQuery.Append(" AND 荷受人ＣＤ LIKE '"+ sTCode + "%' \n");
				}
				if(sTCode.Length > 0 && sKana.Length > 0)
				{
					sbQuery.Append(" AND カナ略称 LIKE '%"+ sKana + "%' \n");
					sbQuery.Append(" AND 荷受人ＣＤ LIKE '"+ sTCode + "%' \n");
				}
				if(sTelNo.Length > 0)
				{
					sbQuery.Append(" AND 電話番号１ LIKE '"+ sTelNo + "%' \n");
				}
				if(sTelNo2.Length > 0)
				{
					sbQuery.Append(" AND 電話番号２ LIKE '"+ sTelNo2 + "%' \n");
				}
				if(sTelNo3.Length > 0)
				{
					sbQuery.Append(" AND 電話番号３ LIKE '"+ sTelNo3 + "%' \n");
				}
				// ADD 2007.01.30 FJCS）桑田 検索条件に名前を追加 START
				if(sName.Length > 0)
				{
					sbQuery.Append(" AND 名前１ LIKE '%"+ sName + "%' \n");
				}
				// ADD 2007.01.30 FJCS）桑田 検索条件に名前を追加 END
// MOD 2010.02.03 東都）高木 検索条件に更新日を追加 START
				if(sUpdateDay.Length > 0){
					string s更新日時Ｓ = sUpdateDay + "000000";
					string s更新日時Ｅ = sUpdateDay + "999999";
					sbQuery.Append(" AND 更新日時 BETWEEN "+s更新日時Ｓ+" AND "+s更新日時Ｅ+" \n");
				}
// MOD 2010.02.03 東都）高木 検索条件に更新日を追加 END

				sbQuery.Append(" AND 削除ＦＧ = '0' \n");
// MOD 2009.01.29 東都）高木 一覧のソート順に[荷受人ＣＤ]を追加 START
//				if((iSortLabel1 != 0)||(iSortLabel2 != 0))
//					sbQuery.Append(" ORDER BY \n");
				sbQuery.Append(" ORDER BY \n");
// MOD 2009.01.29 東都）高木 一覧のソート順に[荷受人ＣＤ]を追加 END
				if(iSortLabel1 != 0)
				{
					switch(iSortLabel1)
					{
// UPD 2007.02.14 FJCS）桑田 Index項目変更 START
//						case 1:
//							sbQuery.Append(" 名前１ ");
//							if(iSortPat1 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 2:
//							sbQuery.Append(" 荷受人ＣＤ ");
//							if(iSortPat1 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 3:
//							sbQuery.Append(" 電話番号１ ");
//							if(iSortPat1 == 1)
//								sbQuery.Append(" DESC \n");
//							sbQuery.Append(", 電話番号２ ");
//							if(iSortPat1 == 1)
//								sbQuery.Append(" DESC \n");
//							sbQuery.Append(", 電話番号３ ");
//							if(iSortPat1 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 4:
//							sbQuery.Append(" カナ略称 ");
//							if(iSortPat1 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 5:
//							sbQuery.Append(" 登録日時 ");
//							if(iSortPat1 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 6:
//							sbQuery.Append(" 更新日時");
//							if(iSortPat1 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
						case 1:
							sbQuery.Append(" カナ略称 ");
							if(iSortPat1 == 1)
								sbQuery.Append(" DESC \n");
							break;
						case 2:
							sbQuery.Append(" 荷受人ＣＤ ");
							if(iSortPat1 == 1)
								sbQuery.Append(" DESC \n");
							break;
						case 3:
							sbQuery.Append(" 電話番号１ ");
							if(iSortPat1 == 1)
								sbQuery.Append(" DESC \n");
							sbQuery.Append(", 電話番号２ ");
							if(iSortPat1 == 1)
								sbQuery.Append(" DESC \n");
							sbQuery.Append(", 電話番号３ ");
							if(iSortPat1 == 1)
								sbQuery.Append(" DESC \n");
							break;
						case 4:
							sbQuery.Append(" 名前１ ");
							if(iSortPat1 == 1)
								sbQuery.Append(" DESC \n");
// ADD 2009.01.29 東都）高木 一覧のソート順に[名前２]を追加 START
							sbQuery.Append(", 名前２ ");
							if(iSortPat1 == 1)
								sbQuery.Append(" DESC \n");
// ADD 2009.01.29 東都）高木 一覧のソート順に[名前２]を追加 END
							break;
						case 5:
							sbQuery.Append(" 登録日時 ");
							if(iSortPat1 == 1)
								sbQuery.Append(" DESC \n");
							break;
						case 6:
							sbQuery.Append(" 更新日時");
							if(iSortPat1 == 1)
								sbQuery.Append(" DESC \n");
							break;
// UPD 2007.02.14 FJCS）桑田 Index項目変更 END
					}
					if(iSortLabel2 != 0)
						sbQuery.Append(" , ");
				}
				if(iSortLabel2 != 0)
				{
					switch(iSortLabel2)
					{
// UPD 2007.02.14 FJCS）桑田 Index項目変更 START
//						case 1:
//							sbQuery.Append(" 名前１ ");
//							if(iSortPat2 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 2:
//							sbQuery.Append(" 荷受人ＣＤ ");
//							if(iSortPat2 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 3:
//							sbQuery.Append(" 電話番号１ ");
//							if(iSortPat2 == 1)
//								sbQuery.Append(" DESC \n");
//							sbQuery.Append(", 電話番号２ ");
//							if(iSortPat2 == 1)
//								sbQuery.Append(" DESC \n");
//							sbQuery.Append(", 電話番号３ ");
//							if(iSortPat2 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 4:
//							sbQuery.Append(" カナ略称 ");
//							if(iSortPat2 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 5:
//							sbQuery.Append(" 登録日時 ");
//							if(iSortPat2 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 6:
///							sbQuery.Append(" 更新日時");
//							if(iSortPat2 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
						case 1:
							sbQuery.Append(" カナ略称 ");
							if(iSortPat2 == 1)
								sbQuery.Append(" DESC \n");
							break;
						case 2:
							sbQuery.Append(" 荷受人ＣＤ ");
							if(iSortPat2 == 1)
								sbQuery.Append(" DESC \n");
							break;
						case 3:
							sbQuery.Append(" 電話番号１ ");
							if(iSortPat2 == 1)
								sbQuery.Append(" DESC \n");
							sbQuery.Append(", 電話番号２ ");
							if(iSortPat2 == 1)
								sbQuery.Append(" DESC \n");
							sbQuery.Append(", 電話番号３ ");
							if(iSortPat2 == 1)
								sbQuery.Append(" DESC \n");
							break;
						case 4:
							sbQuery.Append(" 名前１ ");
							if(iSortPat2 == 1)
								sbQuery.Append(" DESC \n");
// ADD 2009.01.29 東都）高木 一覧のソート順に[名前２]を追加 START
							sbQuery.Append(", 名前２ ");
							if(iSortPat2 == 1)
								sbQuery.Append(" DESC \n");
// ADD 2009.01.29 東都）高木 一覧のソート順に[名前２]を追加 END
							break;
						case 5:
							sbQuery.Append(" 登録日時 ");
							if(iSortPat2 == 1)
								sbQuery.Append(" DESC \n");
							break;
						case 6:
							sbQuery.Append(" 更新日時");
							if(iSortPat2 == 1)
								sbQuery.Append(" DESC \n");
							break;
// UPD 2007.02.14 FJCS）桑田 Index項目変更 END
					}
				}
// ADD 2009.01.29 東都）高木 一覧のソート順に[荷受人ＣＤ]を追加 START
				if((iSortLabel1 != 0) || (iSortLabel2 != 0))
					sbQuery.Append(" , ");
				sbQuery.Append(" 荷受人ＣＤ \n");
// ADD 2009.01.29 東都）高木 一覧のソート順に[荷受人ＣＤ]を追加 END

				OracleDataReader reader;
				reader = CmdSelect(sUser, conn2, sbQuery);

				StringBuilder sbData;
				while (reader.Read())
				{
					sbData = new StringBuilder(1024);
// MOD 2010.02.03 東都）高木 ＣＳＶ出力時に文字項目の前に[']を追加 START
//					sbData.Append(sDbl + reader.GetString(0).Trim() + sDbl);			// 荷受人ＣＤ
//					sbData.Append(sKanma + sDbl + "(" + reader.GetString(1).Trim());	// 電話番号１
//					sbData.Append(")" + reader.GetString(2).Trim() + "-");				// 電話番号２
//					sbData.Append(reader.GetString(3).Trim() + sDbl);					// 電話番号３
//					sbData.Append(sKanma + sDbl);
//					if(reader.GetString(4).Trim().Length > 0) 
//						sbData.Append("(" + reader.GetString(4).Trim() + ")");			// ＦＡＸ番号１
//					if(reader.GetString(5).Trim().Length > 0) 
//						sbData.Append(reader.GetString(5).Trim() + "-");				// ＦＡＸ番号２
//					sbData.Append(reader.GetString(6).Trim() + sDbl);					// ＦＡＸ番号３
//					sbData.Append(sKanma + sDbl + reader.GetString(7).Trim() + sDbl);	// 住所１
//					sbData.Append(sKanma + sDbl + reader.GetString(8).Trim() + sDbl);	// 住所２
//					sbData.Append(sKanma + sDbl + reader.GetString(9).Trim() + sDbl);	// 住所３
//					sbData.Append(sKanma + sDbl + reader.GetString(10).Trim() + sDbl);	// 名前１
//					sbData.Append(sKanma + sDbl + reader.GetString(11).Trim() + sDbl);	// 名前２
//					sbData.Append(sKanma + sDbl + reader.GetString(12).Trim() + sDbl);	// 名前３
//					sbData.Append(sKanma + sDbl + reader.GetString(13).Trim() + sDbl);	// 郵便番号
//					sbData.Append(sKanma + sDbl + reader.GetString(14).Trim() + sDbl);	// カナ略称
//					sbData.Append(sKanma + sDbl + reader.GetString(15).Trim() + sDbl);	// 一斉出荷区分
//					sbData.Append(sKanma + sDbl + reader.GetString(16).Trim() + sDbl);	// 特殊計
//					sbData.Append(sKanma + sDbl + reader.GetString(17).Trim() + sDbl);	// 着店ＣＤ
					sbData.Append(sDbl + sSng + reader.GetString(0).TrimEnd() + sDbl);	// 荷受人ＣＤ
					sbData.Append(sKanma + sDbl + sSng);
					if(reader.GetString(1).TrimEnd().Length > 0) 
						sbData.Append("(" + reader.GetString(1).TrimEnd() + ")");		// 電話番号１
					if(reader.GetString(2).TrimEnd().Length > 0) 
						sbData.Append(reader.GetString(2).TrimEnd() + "-");				// 電話番号２
					sbData.Append(reader.GetString(3).TrimEnd() + sDbl);				// 電話番号３
					sbData.Append(sKanma + sDbl + sSng);
					if(reader.GetString(4).TrimEnd().Length > 0) 
						sbData.Append("(" + reader.GetString(4).TrimEnd() + ")");		// ＦＡＸ番号１
					if(reader.GetString(5).TrimEnd().Length > 0) 
						sbData.Append(reader.GetString(5).TrimEnd() + "-");				// ＦＡＸ番号２
					sbData.Append(reader.GetString(6).TrimEnd() + sDbl);				// ＦＡＸ番号３
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(7).TrimEnd() + sDbl);	// 住所１
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(8).TrimEnd() + sDbl);	// 住所２
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(9).TrimEnd() + sDbl);	// 住所３
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(10).TrimEnd() + sDbl);// 名前１
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(11).TrimEnd() + sDbl);// 名前２
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(12).TrimEnd() + sDbl);// 名前３
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(13).TrimEnd() + sDbl);// 郵便番号
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(14).TrimEnd() + sDbl);// カナ略称
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(15).TrimEnd() + sDbl);// 一斉出荷区分
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(16).TrimEnd() + sDbl);// 特殊計
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(17).TrimEnd() + sDbl);// 着店ＣＤ
// MOD 2010.02.03 東都）高木 ＣＳＶ出力時に文字項目の前に[']を追加 END
					sList.Add(sbData);
				}
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				disposeReader(reader);
				reader = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END

				sRet = new string[sList.Count + 1];
				if(sList.Count == 0) 
					sRet[0] = "該当データがありません";
				else
				{
					sRet[0] = "正常終了";
					int iCnt = 1;
					IEnumerator enumList = sList.GetEnumerator();
					while(enumList.MoveNext())
					{
						sRet[iCnt] = enumList.Current.ToString();
						iCnt++;
					}
				}
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "サーバエラー：" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				conn2 = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
			}
			return sRet;
		}
// ADD 2006.07.05 東都）山本 アドレス帳画面からのＣＳＶ出力対応 END
// ADD 2006.07.10 東都）山本 検索条件に電話番号＆ソート条件の追加 START
// ADD 2007.01.30 FJCS）桑田 検索条件に名前の追加 START
		[WebMethod]
		public String[] Get_todokesaki2(string[] sUser, string sKCode, string sBCode, string sKana, string sTCode, string sTelNo, string sTelNo2, string sTelNo3,string sName, int iSortLabel1,int iSortPat1,int iSortLabel2,int iSortPat2)
// ADD 2007.01.30 FJCS）桑田 検索条件に名前の追加 END
		{
// ADD 2009.01.29 東都）高木 一覧に名前２、住所２、住所３を追加 START
			return Get_todokesaki3(sUser, sKCode, sBCode, sKana, sTCode, sTelNo, sTelNo2, sTelNo3
									, sName, iSortLabel1, iSortPat1, iSortLabel2, iSortPat2, false);
		}

		[WebMethod]
		public String[] Get_todokesaki3(string[] sUser, string sKCode, string sBCode, string sKana, string sTCode, string sTelNo, string sTelNo2, string sTelNo3,string sName, int iSortLabel1,int iSortPat1,int iSortLabel2,int iSortPat2, bool bMultiLine)
// ADD 2007.01.30 FJCS）桑田 検索条件に名前の追加 END
// MOD 2010.02.03 東都）高木 検索条件に更新日を追加 START
		{
			return Get_todokesaki4(sUser, sKCode, sBCode, sKana, sTCode, sTelNo, sTelNo2, sTelNo3
									, sName, iSortLabel1, iSortPat1, iSortLabel2, iSortPat2, false, "");
		}

		[WebMethod]
		public String[] Get_todokesaki4(string[] sUser, string sKCode, string sBCode, string sKana, string sTCode, string sTelNo, string sTelNo2, string sTelNo3,string sName, int iSortLabel1,int iSortPat1,int iSortLabel2,int iSortPat2, bool bMultiLine, string sUpdateDay)
// MOD 2010.02.03 東都）高木 検索条件に更新日を追加 END
		{
// ADD 2009.01.29 東都）高木 一覧に名前２、住所２、住所３を追加 END
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//			logFileOpen(sUser);
			logWriter(sUser, INF, "届け先一覧取得開始");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];
			// ＤＢ接続
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//// ADD 2005.05.23 東都）伊賀 会員チェック追加 START
//			// 会員チェック
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//// ADD 2005.05.23 東都）伊賀 会員チェック追加 END

			StringBuilder sbQuery = new StringBuilder(1024);
			StringBuilder sbRet = new StringBuilder(1024);
			try
			{
				sbQuery.Append(GET_OTODOKE_SELECT);
				sbQuery.Append(" WHERE 会員ＣＤ = '" + sKCode + "' \n");
				sbQuery.Append("   AND 部門ＣＤ = '" + sBCode + "' \n");
				if(sKana.Length > 0 && sTCode.Length == 0)
				{
					sbQuery.Append(" AND カナ略称 LIKE '%"+ sKana + "%' \n");
				}
				if(sTCode.Length > 0 && sKana.Length == 0)
				{
					sbQuery.Append(" AND 荷受人ＣＤ LIKE '"+ sTCode + "%' \n");
				}
				if(sTCode.Length > 0 && sKana.Length > 0)
				{
					sbQuery.Append(" AND カナ略称 LIKE '%"+ sKana + "%' \n");
					sbQuery.Append(" AND 荷受人ＣＤ LIKE '"+ sTCode + "%' \n");
				}
				// ADD 2006.07.04 東都）山本 検索条件に電話番号を追加 START
				if(sTelNo.Length > 0)
				{
					sbQuery.Append(" AND 電話番号１ LIKE '"+ sTelNo + "%' \n");
				}
				if(sTelNo2.Length > 0)
				{
					sbQuery.Append(" AND 電話番号２ LIKE '"+ sTelNo2 + "%' \n");
				}
				if(sTelNo3.Length > 0)
				{
					sbQuery.Append(" AND 電話番号３ LIKE '"+ sTelNo3 + "%' \n");
				}
				// ADD 2006.07.04 東都）山本 検索条件に電話番号を追加 END
				// ADD 2007.01.30 FJCS）桑田 検索条件に名前を追加 START
				if(sName.Length > 0)
				{
					sbQuery.Append(" AND 名前１ LIKE '%"+ sName + "%' \n");
				}
				// ADD 2007.01.30 FJCS）桑田 検索条件に名前を追加 END
// MOD 2010.02.03 東都）高木 検索条件に更新日を追加 START
				if(sUpdateDay.Length > 0){
					string s更新日時Ｓ = sUpdateDay + "000000";
					string s更新日時Ｅ = sUpdateDay + "999999";
					sbQuery.Append(" AND 更新日時 BETWEEN "+s更新日時Ｓ+" AND "+s更新日時Ｅ+" \n");
				}
// MOD 2010.02.03 東都）高木 検索条件に更新日を追加 END

				sbQuery.Append(" AND 削除ＦＧ = '0' \n");
				// MOD 2006.07.04 東都）山本 検索条件にソート機能を追加 START
				//				sbQuery.Append(" ORDER BY 名前１ \n");
// MOD 2009.01.29 東都）高木 一覧のソート順に[荷受人ＣＤ]を追加 START
//				if((iSortLabel1 != 0)||(iSortLabel2 != 0))
//					sbQuery.Append(" ORDER BY \n");
				sbQuery.Append(" ORDER BY \n");
// MOD 2009.01.29 東都）高木 一覧のソート順に[荷受人ＣＤ]を追加 END
				if(iSortLabel1 != 0)
				{
					switch(iSortLabel1)
					{
// UPD 2007.01.30 FJCS）桑田 Index項目変更 START
//						case 1:
//							sbQuery.Append(" 名前１ ");
//							if(iSortPat1 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 2:
//							sbQuery.Append(" 荷受人ＣＤ ");
//							if(iSortPat1 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 3:
//							sbQuery.Append(" 電話番号１ ");
//							if(iSortPat1 == 1)
//								sbQuery.Append(" DESC \n");
//							sbQuery.Append(", 電話番号２ ");
//							if(iSortPat1 == 1)
//								sbQuery.Append(" DESC \n");
//							sbQuery.Append(", 電話番号３ ");
//							if(iSortPat1 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 4:
//							sbQuery.Append(" カナ略称 ");
//							if(iSortPat1 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 5:
//							sbQuery.Append(" 登録日時 ");
//							if(iSortPat1 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 6:
//							sbQuery.Append(" 更新日時");
//							if(iSortPat1 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
						case 1:
							sbQuery.Append(" カナ略称 ");
							if(iSortPat1 == 1)
								sbQuery.Append(" DESC \n");
							break;
						case 2:
							sbQuery.Append(" 荷受人ＣＤ ");
							if(iSortPat1 == 1)
								sbQuery.Append(" DESC \n");
							break;
						case 3:
							sbQuery.Append(" 電話番号１ ");
							if(iSortPat1 == 1)
								sbQuery.Append(" DESC \n");
							sbQuery.Append(", 電話番号２ ");
							if(iSortPat1 == 1)
								sbQuery.Append(" DESC \n");
							sbQuery.Append(", 電話番号３ ");
							if(iSortPat1 == 1)
								sbQuery.Append(" DESC \n");
							break;
						case 4:
							sbQuery.Append(" 名前１ ");
							if(iSortPat1 == 1)
								sbQuery.Append(" DESC \n");
// ADD 2009.01.29 東都）高木 一覧のソート順に[名前２]を追加 START
							sbQuery.Append(", 名前２ ");
							if(iSortPat1 == 1)
								sbQuery.Append(" DESC \n");
// ADD 2009.01.29 東都）高木 一覧のソート順に[名前２]を追加 END
							break;
						case 5:
							sbQuery.Append(" 登録日時 ");
							if(iSortPat1 == 1)
								sbQuery.Append(" DESC \n");
							break;
						case 6:
							sbQuery.Append(" 更新日時");
							if(iSortPat1 == 1)
								sbQuery.Append(" DESC \n");
							break;
// UPD 2007.01.30 FJCS）桑田 Index項目変更 END
					}
					if(iSortLabel2 != 0)
						sbQuery.Append(" , ");
				}
				if(iSortLabel2 != 0)
				{
					switch(iSortLabel2)
					{
// UPD 2007.01.30 FJCS）桑田 Index項目変更 START
//						case 1:
//							sbQuery.Append(" 名前１ ");
//							if(iSortPat2 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 2:
//							sbQuery.Append(" 荷受人ＣＤ ");
//							if(iSortPat2 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 3:
//							sbQuery.Append(" 電話番号１ ");
//							if(iSortPat2 == 1)
//								sbQuery.Append(" DESC \n");
//							sbQuery.Append(", 電話番号２ ");
//							if(iSortPat2 == 1)
//								sbQuery.Append(" DESC \n");
//							sbQuery.Append(", 電話番号３ ");
//							if(iSortPat2 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 4:
//							sbQuery.Append(" カナ略称 ");
//							if(iSortPat2 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 5:
//							sbQuery.Append(" 登録日時 ");
//							if(iSortPat2 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 6:
//							sbQuery.Append(" 更新日時");
//							if(iSortPat2 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
						case 1:
							sbQuery.Append(" カナ略称 ");
							if(iSortPat2 == 1)
								sbQuery.Append(" DESC \n");
							break;
						case 2:
							sbQuery.Append(" 荷受人ＣＤ ");
							if(iSortPat2 == 1)
								sbQuery.Append(" DESC \n");
							break;
						case 3:
							sbQuery.Append(" 電話番号１ ");
							if(iSortPat2 == 1)
								sbQuery.Append(" DESC \n");
							sbQuery.Append(", 電話番号２ ");
							if(iSortPat2 == 1)
								sbQuery.Append(" DESC \n");
							sbQuery.Append(", 電話番号３ ");
							if(iSortPat2 == 1)
								sbQuery.Append(" DESC \n");
							break;
						case 4:
							sbQuery.Append(" 名前１ ");
							if(iSortPat2 == 1)
								sbQuery.Append(" DESC \n");
// ADD 2009.01.29 東都）高木 一覧に名前２、住所２、住所３を追加 START
							sbQuery.Append(", 名前２ ");
							if(iSortPat2 == 1)
								sbQuery.Append(" DESC \n");
// ADD 2009.01.29 東都）高木 一覧に名前２、住所２、住所３を追加 END
							break;
						case 5:
							sbQuery.Append(" 登録日時 ");
							if(iSortPat2 == 1)
								sbQuery.Append(" DESC \n");
							break;
						case 6:
							sbQuery.Append(" 更新日時");
							if(iSortPat2 == 1)
								sbQuery.Append(" DESC \n");
							break;
// UPD 2007.01.30 FJCS）桑田 Index項目変更 END
					}
				}
				// MOD 2006.07.04 東都）山本 検索条件にソート機能を追加 END
// ADD 2009.01.29 東都）高木 一覧のソート順に[荷受人ＣＤ]を追加 START
				if((iSortLabel1 != 0) || (iSortLabel2 != 0))
					sbQuery.Append(" , ");
				sbQuery.Append(" 荷受人ＣＤ \n");
// ADD 2009.01.29 東都）高木 一覧のソート順に[荷受人ＣＤ]を追加 END

				OracleDataReader reader = CmdSelect(sUser, conn2, sbQuery);
				while (reader.Read())
				{
					sbRet = new StringBuilder(1024);

// MOD 2011.01.18 東都）高木 住所名前の前SPACEをつめない START
//					sbRet.Append(sSepa + reader.GetString(0).Trim());
					sbRet.Append(sSepa + reader.GetString(0).TrimEnd()); // 名前１
// MOD 2011.01.18 東都）高木 住所名前の前SPACEをつめない END
// ADD 2009.01.29 東都）高木 一覧に名前２、住所２、住所３を追加 START
					if(bMultiLine){
// MOD 2011.01.18 東都）高木 住所名前の前SPACEをつめない START
//						sbRet.Append(sCRLF + reader.GetString(7).Trim()); // 名前２
						sbRet.Append(sCRLF + reader.GetString(7).TrimEnd()); // 名前２
// MOD 2011.01.18 東都）高木 住所名前の前SPACEをつめない END
					}
// ADD 2009.01.29 東都）高木 一覧に名前２、住所２、住所３を追加 END
// MOD 2011.01.18 東都）高木 住所名前の前SPACEをつめない START
//					sbRet.Append(sSepa + reader.GetString(1).Trim());
					sbRet.Append(sSepa + reader.GetString(1).TrimEnd()); // 住所１
// MOD 2011.01.18 東都）高木 住所名前の前SPACEをつめない END
// ADD 2009.01.29 東都）高木 一覧に名前２、住所２、住所３を追加 START
					if(bMultiLine){
// MOD 2011.01.18 東都）高木 住所名前の前SPACEをつめない START
//						sbRet.Append(sCRLF + reader.GetString(8).Trim()); // 住所２
//						sbRet.Append(sCRLF + reader.GetString(9).Trim()); // 住所３
						sbRet.Append(sCRLF + reader.GetString(8).TrimEnd()); // 住所２
						sbRet.Append(sCRLF + reader.GetString(9).TrimEnd()); // 住所３
// MOD 2011.01.18 東都）高木 住所名前の前SPACEをつめない END
					}
// ADD 2009.01.29 東都）高木 一覧に名前２、住所２、住所３を追加 END
					sbRet.Append(sSepa + reader.GetString(2).Trim());
// MOD 2005.05.11 東都）高木 ORA-03113対策？ START
//					sbRet.Append(sSepa + reader.GetString(3));
//					sbRet.Append(sSepa + reader.GetString(4).Trim());
					sbRet.Append(sSepa + "(" + reader.GetString(3).Trim() + ")"
						+ reader.GetString(4).Trim() + "-"
						+ reader.GetString(5).Trim());	// 電話番号
// MOD 2011.01.18 東都）高木 住所名前の前SPACEをつめない START
//					sbRet.Append(sSepa + reader.GetString(6).Trim());	// カナ略称
					sbRet.Append(sSepa + reader.GetString(6).TrimEnd());// カナ略称
// MOD 2011.01.18 東都）高木 住所名前の前SPACEをつめない END
// MOD 2005.05.11 東都）高木 ORA-03113対策？ END
// MOD 2010.02.02 東都）高木 一覧に登録日時、更新日時、出荷使用日を追加 START
					sbRet.Append(sSepa + reader.GetString(10).TrimEnd());	//郵便番号
					sbRet.Append(sSepa + reader.GetString(11).TrimEnd());	//特殊計
					sbRet.Append(sSepa + reader.GetString(12).TrimEnd());	//メールアドレス
					sbRet.Append(sSepa + reader.GetString(13).TrimEnd());	//住所ＣＤ
					sbRet.Append(sSepa + reader.GetString(14).TrimEnd());	//着店ＣＤ（特殊ＣＤ）
					sbRet.Append(sSepa + ToYYYYMMDD(reader.GetString(15)));	//登録日時
					sbRet.Append(sSepa + ToYYYYMMDD(reader.GetString(16)));	//更新日時
					sbRet.Append(sSepa + ToYYYYMMDD(reader.GetString(17)));	//出荷使用日
// MOD 2010.02.02 東都）高木 一覧に登録日時、更新日時、出荷使用日を追加 END

					sList.Add(sbRet);
				}
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				disposeReader(reader);
				reader = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END

				sRet = new string[sList.Count + 1];
				if(sList.Count == 0) 
					sRet[0] = "該当データがありません";
				else
				{
					sRet[0] = "正常終了";
					int iCnt = 1;
					IEnumerator enumList = sList.GetEnumerator();
					while(enumList.MoveNext())
					{
						sRet[iCnt] = enumList.Current.ToString();
						iCnt++;
					}
				}
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "サーバエラー：" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 START
				conn2 = null;
// ADD 2007.04.28 東都）高木 オブジェクトの破棄 END
// DEL 2007.05.10 東都）高木 未使用関数のコメント化
//				logFileClose();
			}
			
			return sRet;
		}
// ADD 2006.07.10 東都）山本 検索条件に電話番号＆ソート条件の追加 END

// ADD 2008.10.16 kcl)森本 店所コード存在チェック追加 START
		/*********************************************************************
		 * 店所コード存在チェック
		 * 引数：店所ＣＤ
		 * 戻値：ステータス
		 *********************************************************************/
		[WebMethod]
		public string [] Check_TensyoCode(string [] sUser, string sTensyoCode)
		{
			// ログ記録
			logWriter(sUser, INF, "店所コード存在確認開始");

			OracleConnection conn2 = null;
			OracleDataReader reader = null;
			string cmdQuery;
			string [] sRet = new string[1];

			// ＤＢ接続
			conn2 = connect2(sUser);
			if (conn2 == null)
			{
				// ＤＢ接続エラー
				sRet[0] = "ＤＢ接続エラー";
				return sRet;
			}

			try
			{
				// SQL文
				cmdQuery 
					= "SELECT Count(*) "
					+ "  FROM ＣＭ１０店所 CM10 \n"
					+ " WHERE CM10.店所ＣＤ = '" + sTensyoCode + "' \n"
					+ "   AND CM10.削除ＦＧ = '0' \n"
					;

				// SQL実行
				reader = CmdSelect(sUser, conn2, cmdQuery);

				// 実行結果取得
				if (reader.Read())
				{
					// 読み取り成功
					decimal cnt = reader.GetDecimal(0);
					if (cnt > 0m)
					{
						// 店所マスタに登録あり
						sRet[0] = "正常終了";
					} 
					else 
					{
						// 店所マスタに登録なし
						sRet[0] = "店所コードが店所マスタに登録されていません。";
					}
				}
				else
				{
					// 読み取り失敗
					sRet[0] = "店所マスタ読み込み時にエラーが発生しました。";
				}
			}
			catch (OracleException ex)
			{
				// Oracleのエラー
				sRet[0] = chgDBErrMsg(sUser, ex);
				// ログ記録
				logWriter(sUser, ERR, sRet[0]);
			}
			catch (Exception ex)
			{
				// その他のエラー
				sRet[0] = "サーバエラー：" + ex.Message;
				// ログ記録
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				// readerの終了処理
				if (reader != null) 
				{
					disposeReader(reader);
					reader = null;
				}

				// ＤＢ切断
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			
			return sRet;
		}
// ADD 2008.10.16 kcl)森本 店所コード存在チェック追加 END
// MOD 2010.02.02 東都）高木 一覧に登録日時、更新日時、出荷使用日を追加 START
		private string ToYYYYMMDD(string sDate)
		{
			string sRet = "";
			sDate = sDate.TrimEnd();
			if(sDate.Length < 8) return sRet;
			sRet += sDate.Substring( 0,4);
//			sRet += sDate.Substring( 2,2);
			sRet += "/";
			sRet += sDate.Substring( 4,2);
			sRet += "/";
			sRet += sDate.Substring( 6,2);
			if(sDate.Length < 14) return sRet;
			sRet += " ";
			sRet += sDate.Substring( 8,2);
			sRet += ":";
			sRet += sDate.Substring(10,2);
			sRet += ":";
			sRet += sDate.Substring(12,2);
			return sRet;
		}
// MOD 2010.02.02 東都）高木 一覧に登録日時、更新日時、出荷使用日を追加 END
	}
}

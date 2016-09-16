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
	// �C������
	//--------------------------------------------------------------------------
	// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j��
	//	disposeReader(reader);
	//	reader = null;
	//--------------------------------------------------------------------------
	// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
	//	logFileOpen(sUser);
	//	userCheck2(conn2, sUser);
	//	logFileClose();
	//--------------------------------------------------------------------------
	// MOD 2008.06.11 kcl)�X�{ ���X�R�[�h�������@�̕ύX�ɔ�������b�c�̓o�^��ǉ�
	//  Get_Stodokesaki
	//  Get_csvwrite
	//  Get_csvwrite2
	//  Ins_todokesaki
	//  Upd_todokesaki
	//  Ins_uploadData
	//--------------------------------------------------------------------------
	// ADD 2008.10.16 kcl)�X�{ ���X�R�[�h���݃`�F�b�N�ǉ� 
	//--------------------------------------------------------------------------
	// ADD 2009.01.29 ���s�j���� �ꗗ�ɖ��O�Q�A�Z���Q�A�Z���R��ǉ� 
	// ADD 2009.01.29 ���s�j���� �ꗗ�̃\�[�g����[�׎�l�b�c]��ǉ� 
	// ADD 2009.01.29 ���s�j���� �ꗗ�̃\�[�g����[���O�Q]��ǉ� 
	// ADD 2009.01.30 ���s�j���� [���O�R]�ɍŏI���p�N�����X�V 
	//--------------------------------------------------------------------------
	// MOD 2010.02.02 ���s�j���� �ꗗ�ɓo�^�����A�X�V�����A�o�׎g�p����ǉ� 
	// MOD 2010.02.02 ���s�j���� �׎�l�}�X�^��[�o�^�o�f]�ɏo�׎g�p�����X�V 
	// MOD 2010.02.03 ���s�j���� ���������ɍX�V����ǉ� 
	// MOD 2010.02.03 ���s�j���� �b�r�u�o�͎��ɕ������ڂ̑O��[']��ǉ� 
	// MOD 2010.03.11 ���s�j���� �b�r�u�捞���̗X�֔ԍ��P�O�O���ꊇ�`�F�b�N��ǉ� 
	//�ۗ� MOD 2010.04.13 ���s�j���� �X�֔ԍ����폜���ꂽ���̏�Q�Ή� 
	// MOD 2010.05.10 ���s�j���� �o�f��Q�̏C�� 
	// MOD 2010.06.03 ���s�j���� �������폜�@�\�̍����� 
	//--------------------------------------------------------------------------
	// MOD 2011.01.18 ���s�j���� �Z�����O�̑OSPACE���߂Ȃ� 
	//--------------------------------------------------------------------------
	[System.Web.Services.WebService(
		 Namespace="http://Walkthrough/XmlWebServices/",
		 Description="is2otodoke")]

	public class Service1 : is2common.CommService
	{
// ADD 2009.01.29 ���s�j���� �ꗗ�ɖ��O�Q�A�Z���Q�A�Z���R��ǉ� START
		private static string sCRLF = "\\r\\n";
// ADD 2009.01.29 ���s�j���� �ꗗ�ɖ��O�Q�A�Z���Q�A�Z���R��ǉ� END
		private static string sSepa = "|";
		private static string sKanma = ",";
		private static string sDbl = "\"";
// MOD 2010.02.03 ���s�j���� �b�r�u�o�͎��ɕ������ڂ̑O��[']��ǉ� START
		private static string sSng = "'";
// MOD 2010.02.03 ���s�j���� �b�r�u�o�͎��ɕ������ڂ̑O��[']��ǉ� END

		public Service1()
		{
			//CODEGEN: ���̌Ăяo���́AASP.NET Web �T�[�r�X �f�U�C�i�ŕK�v�ł��B
			InitializeComponent();

			connectService();
		}

		#region �R���|�[�l���g �f�U�C�i�Ő������ꂽ�R�[�h 
		
		//Web �T�[�r�X �f�U�C�i�ŕK�v�ł��B
		private IContainer components = null;
				
		/// <summary>
		/// �f�U�C�i �T�|�[�g�ɕK�v�ȃ��\�b�h�ł��B���̃��\�b�h�̓��e��
		/// �R�[�h �G�f�B�^�ŕύX���Ȃ��ł��������B
		/// </summary>
		private void InitializeComponent()
		{
		}

		/// <summary>
		/// �g�p����Ă��郊�\�[�X�Ɍ㏈�������s���܂��B
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
		 * �͂���ꗗ�擾
		 * �����F����b�c�A����b�c�A�J�i�A�׎�l�b�c
		 * �ߒl�F�X�e�[�^�X�A�ꗗ�i���O�P�A�Z���P�A�׎�l�b�c�A�d�b�ԍ��A�J�i���́j
		 *
		 *********************************************************************/
// MOD 2005.05.11 ���s�j���� ORA-03113�΍�H START
//		private static string GET_OTODOKE_SELECT
//			= "SELECT ���O�P,�Z���P,�׎�l�b�c,'(' || TRIM(�d�b�ԍ��P) || ')' \n"
//			+       " || TRIM(�d�b�ԍ��Q) || '-' || TRIM(�d�b�ԍ��R),�J�i���� \n"
//			+ " FROM �r�l�O�Q�׎�l \n";
		private static string GET_OTODOKE_SELECT
			= "SELECT /*+ INDEX(�r�l�O�Q�׎�l SM02PKEY) */ \n"
			+ " ���O�P, �Z���P, �׎�l�b�c, �d�b�ԍ��P, \n"
			+ " �d�b�ԍ��Q, �d�b�ԍ��R, �J�i���� \n"
// ADD 2009.01.29 ���s�j���� �ꗗ�ɖ��O�Q�A�Z���Q�A�Z���R��ǉ� START
			+ ", ���O�Q, �Z���Q, �Z���R \n"
// ADD 2009.01.29 ���s�j���� �ꗗ�ɖ��O�Q�A�Z���Q�A�Z���R��ǉ� END
// MOD 2010.02.02 ���s�j���� �ꗗ�ɓo�^�����A�X�V�����A�o�׎g�p����ǉ� START
			+ ", �X�֔ԍ�, ����v, \"���[���A�h���X\", �Z���b�c, ����b�c \n"
			+ ", TO_CHAR(�o�^����), TO_CHAR(�X�V����), �o�^�o�f \n"
// MOD 2010.02.02 ���s�j���� �ꗗ�ɓo�^�����A�X�V�����A�o�׎g�p����ǉ� END
			+ " FROM �r�l�O�Q�׎�l \n";
// MOD 2005.05.11 ���s�j���� ORA-03113�΍�H END

		[WebMethod]
		public String[] Get_todokesaki(string[] sUser, string sKCode, string sBCode, string sKana, string sTCode)
			{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//			logFileOpen(sUser);
			logWriter(sUser, INF, "�͂���ꗗ�擾�J�n");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];
			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//// ADD 2005.05.23 ���s�j�ɉ� ����`�F�b�N�ǉ� START
//			// ����`�F�b�N
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//// ADD 2005.05.23 ���s�j�ɉ� ����`�F�b�N�ǉ� END

			StringBuilder sbQuery = new StringBuilder(1024);
			StringBuilder sbRet = new StringBuilder(1024);
			try
			{
				sbQuery.Append(GET_OTODOKE_SELECT);
				sbQuery.Append(" WHERE ����b�c = '" + sKCode + "' \n");
				sbQuery.Append("   AND ����b�c = '" + sBCode + "' \n");
				if(sKana.Length > 0 && sTCode.Length == 0)
				{
					sbQuery.Append(" AND �J�i���� LIKE '%"+ sKana + "%' \n");
				}
				if(sTCode.Length > 0 && sKana.Length == 0)
				{
					sbQuery.Append(" AND �׎�l�b�c LIKE '"+ sTCode + "%' \n");
				}
				if(sTCode.Length > 0 && sKana.Length > 0)
				{
					sbQuery.Append(" AND �J�i���� LIKE '%"+ sKana + "%' \n");
					sbQuery.Append(" AND �׎�l�b�c LIKE '"+ sTCode + "%' \n");
				}
				sbQuery.Append(" AND �폜�e�f = '0' \n");
				sbQuery.Append(" ORDER BY ���O�P \n");
// ADD 2009.01.29 ���s�j���� �ꗗ�̃\�[�g����[���O�Q]��ǉ� START
				sbQuery.Append(", ���O�Q ");
// ADD 2009.01.29 ���s�j���� �ꗗ�̃\�[�g����[���O�Q]��ǉ� END
// ADD 2009.01.29 ���s�j���� �ꗗ�̃\�[�g����[�׎�l�b�c]��ǉ� START
				sbQuery.Append(", �׎�l�b�c \n");
// ADD 2009.01.29 ���s�j���� �ꗗ�̃\�[�g����[�׎�l�b�c]��ǉ� END

				OracleDataReader reader = CmdSelect(sUser, conn2, sbQuery);
				while (reader.Read())
				{
					sbRet = new StringBuilder(1024);

// MOD 2011.01.18 ���s�j���� �Z�����O�̑OSPACE���߂Ȃ� START
//					sbRet.Append(sSepa + reader.GetString(0).Trim());
//					sbRet.Append(sSepa + reader.GetString(1).Trim());
					sbRet.Append(sSepa + reader.GetString(0).TrimEnd()); // ���O�P
					sbRet.Append(sSepa + reader.GetString(1).TrimEnd()); // �Z���P
//�ۗ� MOD 2011.01.18 ���s�j���� �Z�����O�̑OSPACE���߂Ȃ� END
					sbRet.Append(sSepa + reader.GetString(2).Trim());
// MOD 2005.05.11 ���s�j���� ORA-03113�΍�H START
//					sbRet.Append(sSepa + reader.GetString(3));
//					sbRet.Append(sSepa + reader.GetString(4).Trim());
					sbRet.Append(sSepa + "(" + reader.GetString(3).Trim() + ")"
										+ reader.GetString(4).Trim() + "-"
										+ reader.GetString(5).Trim());	// �d�b�ԍ�
// MOD 2011.01.18 ���s�j���� �Z�����O�̑OSPACE���߂Ȃ� START
//					sbRet.Append(sSepa + reader.GetString(6).Trim());	// �J�i����
					sbRet.Append(sSepa + reader.GetString(6).TrimEnd());// �J�i����
// MOD 2011.01.18 ���s�j���� �Z�����O�̑OSPACE���߂Ȃ� END
// MOD 2005.05.11 ���s�j���� ORA-03113�΍�H END

					sList.Add(sbRet);
				}
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				disposeReader(reader);
				reader = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END

				sRet = new string[sList.Count + 1];
				if(sList.Count == 0) 
					sRet[0] = "�Y���f�[�^������܂���";
				else
				{
					sRet[0] = "����I��";
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
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				conn2 = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
			}
			
			return sRet;
		}

		/*********************************************************************
		 * �͂���f�[�^�擾
		 * �����F����b�c�A����b�c�A�׎�l�b�c
		 * �ߒl�F�X�e�[�^�X�A�J�i���́A�d�b�ԍ��A�X�֔ԍ��A�Z���A���O�A����v
		 *		 ���[���A�h���X�A�X�V����
		 *********************************************************************/
// MOD 2007.02.13 ���s�j���� �m�t�k�k�G���[�΍� START
//// ADD 2005.05.11 ���s�j���� ORA-03113�΍�H START
//		private static string GET_STODOKESAKI_SELECT
//			= "SELECT �J�i����, �d�b�ԍ��P, �d�b�ԍ��Q, �d�b�ԍ��R, \n"
//			+ " �X�֔ԍ�, �Z���P, �Z���Q, �Z���R, \n"
//			+ " ���O�P, ���O�Q, ����v, \"���[���A�h���X\", �X�V���� \n"
//			+ " FROM �r�l�O�Q�׎�l \n";
//// ADD 2005.05.11 ���s�j���� ORA-03113�΍�H END
		private static string GET_STODOKESAKI_SELECT
			= "SELECT �J�i����, �d�b�ԍ��P, �d�b�ԍ��Q, �d�b�ԍ��R, \n"
			+ " �X�֔ԍ�, �Z���P, �Z���Q, �Z���R, \n"
			+ " ���O�P, ���O�Q, ����v, \"���[���A�h���X\", TO_CHAR(�X�V����) \n"
// ADD 2008.06.11 kcl)�X�{ ���X�R�[�h�������@�̕ύX START
			+ ",�Z���b�c, ����b�c \n"
// ADD 2008.06.11 kcl)�X�{ ���X�R�[�h�������@�̕ύX END
			+ " FROM �r�l�O�Q�׎�l \n";
// MOD 2007.02.13 ���s�j���� �m�t�k�k�G���[�΍� END
		[WebMethod]
		public String[] Get_Stodokesaki(string[] sUser, string sKCode,string sBCode,string sTCode)
		{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//			logFileOpen(sUser);
			logWriter(sUser, INF, "�͂�����擾�J�n");

			OracleConnection conn2 = null;
// MOD 2008.06.11 kcl)�X�{ ���X�R�[�h�������@�̕ύX START
//			string[] sRet = new string[16];
			string[] sRet = new string[18];
// MOD 2008.06.11 kcl)�X�{ ���X�R�[�h�������@�̕ύX END
			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//// ADD 2005.05.23 ���s�j�ɉ� ����`�F�b�N�ǉ� START
//			// ����`�F�b�N
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//// ADD 2005.05.23 ���s�j�ɉ� ����`�F�b�N�ǉ� END

			try
			{
// MOD 2005.05.11 ���s�j���� ORA-03113�΍�H START
//				string cmdQuery = "SELECT �J�i����,�d�b�ԍ��P,�d�b�ԍ��Q,�d�b�ԍ��R,"
//					+ "SUBSTR(�X�֔ԍ�,1,3),SUBSTR(�X�֔ԍ�,4,4),�Z���P,�Z���Q,�Z���R,"
//					+ "���O�P,���O�Q,����v, \"���[���A�h���X\", TO_CHAR(�X�V����) \n"
//					+ "  FROM �r�l�O�Q�׎�l \n"
//					+ " WHERE �׎�l�b�c = '" + sTCode + "' AND ����b�c = '" + sKCode + "' \n"
//					+ "   AND ����b�c   = '" + sBCode + "' AND �폜�e�f = '0'";
				string cmdQuery = GET_STODOKESAKI_SELECT
					+ " WHERE �׎�l�b�c = '" + sTCode + "' \n"
					+ " AND ����b�c = '" + sKCode + "' \n"
					+ " AND ����b�c = '" + sBCode + "' \n"
					+ " AND �폜�e�f = '0' \n";
// MOD 2005.05.11 ���s�j���� ORA-03113�΍�H END

				OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);

				bool bRead = reader.Read();
				if(bRead == true)
				{
// MOD 2005.05.11 ���s�j���� ORA-03113�΍�H START
//					for(int iCnt = 1; iCnt < 15; iCnt++)
//					{
//						sRet[iCnt] = reader.GetString(iCnt - 1).Trim();
//					}
// MOD 2011.01.18 ���s�j���� �Z�����O�̑OSPACE���߂Ȃ� START
//					sRet[1]  = reader.GetString(0).Trim();
					sRet[1]  = reader.GetString(0).TrimEnd(); // �J�i����
// MOD 2011.01.18 ���s�j���� �Z�����O�̑OSPACE���߂Ȃ� END
					sRet[2]  = reader.GetString(1).Trim();
					sRet[3]  = reader.GetString(2).Trim();
					sRet[4]  = reader.GetString(3).Trim();
					sRet[5]  = reader.GetString(4).Trim();	// �X�֔ԍ�
					sRet[6]  = "";
					if(sRet[5].Length > 3)
					{
						sRet[6]  = sRet[5].Substring(3);
						sRet[5]  = sRet[5].Substring(0,3);
					}
// MOD 2011.01.18 ���s�j���� �Z�����O�̑OSPACE���߂Ȃ� START
//					sRet[7]  = reader.GetString(5).Trim();
//					sRet[8]  = reader.GetString(6).Trim();
//					sRet[9]  = reader.GetString(7).Trim();
//					sRet[10] = reader.GetString(8).Trim();
//					sRet[11] = reader.GetString(9).Trim();
					sRet[7]  = reader.GetString(5).TrimEnd(); // �Z���P
					sRet[8]  = reader.GetString(6).TrimEnd(); // �Z���Q
					sRet[9]  = reader.GetString(7).TrimEnd(); // �Z���R
					sRet[10] = reader.GetString(8).TrimEnd(); // ���O�P
					sRet[11] = reader.GetString(9).TrimEnd(); // ���O�Q
// MOD 2011.01.18 ���s�j���� �Z�����O�̑OSPACE���߂Ȃ� END
					sRet[12] = reader.GetString(10).Trim();
					sRet[13] = reader.GetString(11).Trim();
// MOD 2007.02.13 ���s�j���� �m�t�k�k�G���[�΍� START
//					sRet[14] = reader.GetDecimal(12).ToString().Trim();	// �X�V����
					sRet[14] = reader.GetString(12).Trim();	// �X�V����
// MOD 2007.02.13 ���s�j���� �m�t�k�k�G���[�΍� END
// MOD 2005.05.11 ���s�j���� ORA-03113�΍�H END
					sRet[0] = "�X�V";
					sRet[15] = "U";
// ADD 2008.06.11 kcl)�X�{ ���X�R�[�h�������@�̕ύX START
					sRet[16] = reader.GetString(13).Trim();
					sRet[17] = reader.GetString(14).Trim();
// ADD 2008.06.11 kcl)�X�{ ���X�R�[�h�������@�̕ύX END
				}
				else
				{
					sRet[0] = "�o�^";
					sRet[15] = "I";
				}
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				disposeReader(reader);
				reader = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END

			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				conn2 = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
			}
			
			return sRet;
		}

		/*********************************************************************
		 * �͂���f�[�^�X�V
		 * �����F����b�c�A����b�c�A�׎�l�b�c...
		 * �ߒl�F�X�e�[�^�X
		 *
		 *********************************************************************/
		[WebMethod]
		public String[] Upd_todokesaki(string[] sUser, string[] sData)
		{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//			logFileOpen(sUser);
			logWriter(sUser, INF, "�͂���X�V�J�n");

			OracleConnection conn2 = null;
			string[] sRet = new string[5];
			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//// ADD 2005.05.23 ���s�j�ɉ� ����`�F�b�N�ǉ� START
//			// ����`�F�b�N
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//// ADD 2005.05.23 ���s�j�ɉ� ����`�F�b�N�ǉ� END

// ADD 2006.07.10 ���s�j���� �Z���b�c�̒ǉ� START
			string s�Z���b�c = " ";
			if(sData.Length >= 20 && sData[19].Length > 0){
				s�Z���b�c = sData[19];
			}
// ADD 2006.07.10 ���s�j���� �Z���b�c�̒ǉ� END
// ADD 2008.06.11 kcl)�X�{ ���X�R�[�h�������@�̕ύX START
			string s����b�c = " ";
			if (sData.Length >= 21 && sData[20].Length > 0) 
			{
				s����b�c = sData[20];
			}
// ADD 2008.06.11 kcl)�X�{ ���X�R�[�h�������@�̕ύX END

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			try
			{
				string cmdQuery 
					= "UPDATE �r�l�O�Q�׎�l \n"
					+    "SET �J�i����           = '" + sData[1]  +"', \n"
					+        "�d�b�ԍ��P         = '" + sData[2]  +"', \n"
					+        "�d�b�ԍ��Q         = '" + sData[3]  +"', \n"
					+        "�d�b�ԍ��R         = '" + sData[4]  +"', \n"
					+        "�X�֔ԍ�           = '" + sData[5] + sData[6] +"', \n"
					+        "�Z���P             = '" + sData[7]  +"', \n"
					+        "�Z���Q             = '" + sData[8]  +"', \n"
					+        "�Z���R             = '" + sData[9]  +"', \n"
					+        "���O�P             = '" + sData[10] +"', \n"
					+        "���O�Q             = '" + sData[11] +"', \n"
// MOD 2010.02.02 ���s�j���� �׎�l�}�X�^��[�o�^�o�f]�ɏo�׎g�p�����X�V START
//// ADD 2009.01.30 ���s�j���� [���O�R]�ɍŏI���p�N�����X�V START
//					+        "���O�R             = TO_CHAR(SYSDATE,'YYYYMM'), \n"
//// ADD 2009.01.30 ���s�j���� [���O�R]�ɍŏI���p�N�����X�V END
// MOD 2010.02.02 ���s�j���� �׎�l�}�X�^��[�o�^�o�f]�ɏo�׎g�p�����X�V END
// MOD 2006.07.10 ���s�j���� �Z���b�c�̒ǉ� START
					+        "�Z���b�c           = '" + s�Z���b�c +"', \n"
// MOD 2006.07.10 ���s�j���� �Z���b�c�̒ǉ� END
// ADD 2008.06.11 kcl)�X�{ ���X�R�[�h�������@�̕ύX START
					+        "����b�c           = '" + s����b�c +"', \n"
// ADD 2008.06.11 kcl)�X�{ ���X�R�[�h�������@�̕ύX END
					+        "����v             = '" + sData[12] +"', \n"
					+        "\"���[���A�h���X\" = '" + sData[13] +"', \n"
					+        "�폜�e�f           = '0', \n"
// MOD 2010.02.02 ���s�j���� �׎�l�}�X�^��[�o�^�o�f]�ɏo�׎g�p�����X�V START
					+        "�o�^�o�f           = ' ', \n"
// MOD 2010.02.02 ���s�j���� �׎�l�}�X�^��[�o�^�o�f]�ɏo�׎g�p�����X�V END
					+        "�X�V�o�f           = '" + sData[14] +"', \n"
					+        "�X�V��             = '" + sData[15] +"', \n"
					+        "�X�V����           =  TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') \n"
					+ " WHERE ����b�c           = '" + sData[16] +"' \n"
					+ "   AND ����b�c           = '" + sData[17] +"' \n"
					+ "   AND �׎�l�b�c         = '" + sData[0] +"' \n"
					+ "   AND �X�V����           =  " + sData[18] +"";

				int iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);
				tran.Commit();
				if(iUpdRow == 0)
					sRet[0] = "�f�[�^�ҏW���ɑ��̒[�����X�V����Ă��܂��B\r\n�ēx�A�ŐV�f�[�^���Ăяo���čX�V���Ă��������B";
				else				
					sRet[0] = "����I��";
			}
			catch (OracleException ex)
			{
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				tran.Rollback();
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				conn2 = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
			}
			
			return sRet;
		}

		/*********************************************************************
		 * �͂���f�[�^�o�^
		 * �����F����b�c�A����b�c�A�׎�l�b�c...
		 * �ߒl�F�X�e�[�^�X
		 *
		 *********************************************************************/
		[WebMethod]
		public String[] Ins_todokesaki(string[] sUser, string[] sData)
		{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//			logFileOpen(sUser);
			logWriter(sUser, INF, "�͂���o�^�J�n");

			OracleConnection conn2 = null;
			string[] sRet = new string[5];
			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//// ADD 2005.05.23 ���s�j�ɉ� ����`�F�b�N�ǉ� START
//			// ����`�F�b�N
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//// ADD 2005.05.23 ���s�j�ɉ� ����`�F�b�N�ǉ� END

// ADD 2006.07.10 ���s�j���� �Z���b�c�̒ǉ� START
			string s�Z���b�c = " ";
			if(sData.Length >= 20 && sData[19].Length > 0){
				s�Z���b�c = sData[19];
			}
// ADD 2006.07.10 ���s�j���� �Z���b�c�̒ǉ� END
// ADD 2008.06.11 kcl)�X�{ ���X�R�[�h�������@�̕ύX START
			string s����b�c = " ";
			if (sData.Length >= 21 && sData[20].Length > 0) 
			{
				s����b�c = sData[20];
			}
// ADD 2008.06.11 kcl)�X�{ ���X�R�[�h�������@�̕ύX END

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			try
			{
				string cmdQuery 
					= "DELETE FROM �r�l�O�Q�׎�l \n"
					+ " WHERE ����b�c           = '" + sData[16] +"' \n"
					+ "   AND ����b�c           = '" + sData[17] +"' \n"
					+ "   AND �׎�l�b�c         = '" + sData[0] +"' \n"
					+ "   AND �폜�e�f           = '1'";

				int iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);

				cmdQuery 
					= "INSERT INTO �r�l�O�Q�׎�l \n"
					+ "VALUES ('" + sData[16] +"','" + sData[17] +"','" + sData[0] +"','" + sData[2] +"','" + sData[3] +"', \n"
					+         "'" + sData[4] +"',' ',' ',' ','" + sData[7] +"','" + sData[8] +"', \n"
// MOD 2006.07.10 ���s�j���� �Z���b�c�̒ǉ� START
//					+         "'" + sData[9] +"','" + sData[10] +"','" + sData[11] +"',' ','" + sData[5] + sData[6] +"',' ', \n"
// MOD 2010.02.02 ���s�j���� �׎�l�}�X�^��[�o�^�o�f]�ɏo�׎g�p�����X�V START
//// MOD 2009.01.30 ���s�j���� [���O�R]�ɍŏI���p�N�����X�V START
////					+         "'" + sData[9] +"','" + sData[10] +"','" + sData[11] +"',' ','" + sData[5] + sData[6] +"','" + s�Z���b�c + "', \n"
//					+         "'" + sData[9] +"','" + sData[10] +"','" + sData[11] +"',TO_CHAR(SYSDATE,'YYYYMM'),'" + sData[5] + sData[6] +"','" + s�Z���b�c + "', \n"
//// MOD 2009.01.30 ���s�j���� [���O�R]�ɍŏI���p�N�����X�V END
					+         "'" + sData[9] +"','" + sData[10] +"','" + sData[11] +"',' ','" + sData[5] + sData[6] +"','" + s�Z���b�c + "', \n"
// MOD 2010.02.02 ���s�j���� �׎�l�}�X�^��[�o�^�o�f]�ɏo�׎g�p�����X�V END
// MOD 2006.07.10 ���s�j���� �Z���b�c�̒ǉ� END
// MOD 2008.06.11 kcl)�X�{ ���X�R�[�h�������@�̕ύX START
//					+         "'" + sData[1] +"',' ',' ','" + sData[12] +"','" + sData[13] +"', \n"
					+         "'" + sData[1] +"',' ','" + s����b�c + "','" + sData[12] +"','" + sData[13] +"', \n"
// MOD 2008.06.11 kcl)�X�{ ���X�R�[�h�������@�̕ύX END
// MOD 2010.02.02 ���s�j���� �׎�l�}�X�^��[�o�^�o�f]�ɏo�׎g�p�����X�V START
//					+         "'0',TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'),'" + sData[14] +"','" + sData[15] +"', \n"
					+         "'0',TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'),' ','" + sData[15] +"', \n"
// MOD 2010.02.02 ���s�j���� �׎�l�}�X�^��[�o�^�o�f]�ɏo�׎g�p�����X�V END
					+         "TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'),'" + sData[14] +"','" + sData[15] +"')";

				iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);
				tran.Commit();
				sRet[0] = "����I��";
				
			}
			catch (OracleException ex)
			{
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				tran.Rollback();
// DEL 2005.05.31 ���s�j���� �s�v�Ȉ׍폜 START
//				string sErr = ex.Message.Substring(0,9);
//				if(sErr == "ORA-00001")
//					sRet[0] = "����̃R�[�h�����ɑ��̒[�����o�^����Ă��܂��B\r\n�ēx�A�ŐV�f�[�^���Ăяo���čX�V���Ă��������B";
//				else
// DEL 2005.05.31 ���s�j���� �s�v�Ȉ׍폜 END
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				conn2 = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
			}
			
			return sRet;
		}

// MOD 2010.06.03 ���s�j���� �������폜�@�\�̍����� START
		/*********************************************************************
		 * �͂���f�[�^�폜
		 * �����F����b�c�A����b�c�A�׎�l�b�c�A�X�V�o�f�A�X�V��
		 * �ߒl�F�X�e�[�^�X
		 *
		 *********************************************************************/
		[WebMethod]
		public String[] Del_todokesakis(string[] sUser, string[] sData, string[] sList)
		{
			logWriter(sUser, INF, "�͂��敡�����폜�J�n");

			OracleConnection conn2 = null;
			string[] sRet = new string[sList.Length + 1];
			sRet[0] = "";
			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null){
				sRet[0] = "�c�a�ڑ��G���[";
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
						= "UPDATE �r�l�O�Q�׎�l \n"
						+    "SET �폜�e�f           = '1', \n"
						+        "�X�V�o�f           = '" + sData[3] +"', \n"
						+        "�X�V��             = '" + sData[4] +"', \n"
						+        "�X�V����           =  TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') \n"
						+ " WHERE ����b�c           = '" + sData[0] +"' \n"
						+ "   AND ����b�c           = '" + sData[1] +"' \n"
						+ "   AND �׎�l�b�c         = '" + sList[iCnt] +"'";
					int iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);
					sRet[iCnt + 1] = iUpdRow.ToString();
				}

				tran.Commit();				
				sRet[0] = "����I��";
			}catch (OracleException ex){
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}catch (Exception ex){
				tran.Rollback();
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}finally{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			
			return sRet;
		}

// MOD 2010.06.03 ���s�j���� �������폜�@�\�̍����� END
		/*********************************************************************
		 * �͂���f�[�^�폜
		 * �����F����b�c�A����b�c�A�׎�l�b�c�A�X�V�o�f�A�X�V��
		 * �ߒl�F�X�e�[�^�X
		 *
		 *********************************************************************/
		[WebMethod]
		public String[] Del_todokesaki(string[] sUser, string[] sData)
		{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//			logFileOpen(sUser);
			logWriter(sUser, INF, "�͂���폜�J�n");

			OracleConnection conn2 = null;
			string[] sRet = new string[5];
			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//// ADD 2005.05.23 ���s�j�ɉ� ����`�F�b�N�ǉ� START
//			// ����`�F�b�N
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//// ADD 2005.05.23 ���s�j�ɉ� ����`�F�b�N�ǉ� END

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			try
			{
				string cmdQuery 
					= "UPDATE �r�l�O�Q�׎�l \n"
					+    "SET �폜�e�f           = '1', \n"
					+        "�X�V�o�f           = '" + sData[3] +"', \n"
					+        "�X�V��             = '" + sData[4] +"', \n"
					+        "�X�V����           =  TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') \n"
					+ " WHERE ����b�c           = '" + sData[0] +"' \n"
					+ "   AND ����b�c           = '" + sData[1] +"' \n"
					+ "   AND �׎�l�b�c         = '" + sData[2] +"'";

				int iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);
				tran.Commit();				
				sRet[0] = "����I��";
				
			}
			catch (OracleException ex)
			{
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				tran.Rollback();
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				conn2 = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
			}
			
			return sRet;
		}

		// ADD Hijiya Start  �͂���폜�Q
		[WebMethod]
		public String[] Del_todokesaki2(string[] sUser, string sKCode, string sBCode, string sTCode)
		{
			OracleConnection conn2 = null;
			string[] sRet = new string[5];
			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//// ADD 2005.05.23 ���s�j�ɉ� ����`�F�b�N�ǉ� START
//			// ����`�F�b�N
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//// ADD 2005.05.23 ���s�j�ɉ� ����`�F�b�N�ǉ� END

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			try
			{
				string cmdQuery 
					= "DELETE FROM �r�l�O�Q�׎�l "
					+ " WHERE ����b�c           = '" + sKCode +"'"
					+ "   AND ����b�c           = '" + sBCode +"'"
					+ "   AND �׎�l�b�c         = '" + sTCode +"'"
					+ "   AND �폜�e�f           = '1'";

// MOD 2005.06.08 ���s�j���� �c�a�ڑ����@�̕ύX START
//				OracleCommand cmd = new OracleCommand(cmdQuery);
//				cmd.Connection = conn;
//				cmd.CommandType = CommandType.Text;
//
//				cmd.ExecuteNonQuery();
				int iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);
// MOD 2005.06.08 ���s�j���� �c�a�ڑ����@�̕ύX END
				tran.Commit();				
				sRet[0] = "1";
				
			}
// ADD 2005.05.27 ���s�j���� ���b�Z�[�W�̕ύX START
			catch (OracleException ex)
			{
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
// ADD 2005.05.27 ���s�j���� ���b�Z�[�W�̕ύX END
			catch (Exception ex)
			{
				tran.Rollback();
// MOD 2005.05.27 ���s�j���� ���b�Z�[�W�̕ύX START
//				sRet[0] = "�c�a�G���[�F" + ex.Message;
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
// MOD 2005.05.27 ���s�j���� ���b�Z�[�W�̕ύX END
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				conn2 = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
			}

			return sRet;
		}
		// ADD Hijiya End

// ADD 2006.12.14 ���s�j�����J ���͐���S���폜 START
		/*********************************************************************
		 * �͂���f�[�^�폜�R
		 * �����F����b�c�A����b�c�A�X�V�o�f�A�X�V��
		 * �ߒl�F�X�e�[�^�X
		 *
		 *********************************************************************/
		[WebMethod]
		public String[] Del_todokesaki3(string[] sUser, string[] sData)
		{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//			logFileOpen(sUser);
			logWriter(sUser, INF, "�͂���S���폜�J�n");

			OracleConnection conn2 = null;
			string[] sRet = new string[5];
			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//
//			// ����`�F�b�N
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
					= "UPDATE �r�l�O�Q�׎�l \n"
					+    "SET �폜�e�f           = '1', \n"
					+        "�X�V�o�f           = '" + sData[2] +"', \n"
					+        "�X�V��             = '" + sData[3] +"', \n"
					+        "�X�V����           =  TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') \n"
					+ " WHERE ����b�c           = '" + sData[0] +"' \n"
					+ "   AND ����b�c           = '" + sData[1] +"'"
					+ "   AND �폜�e�f           = '0'";

				int iUpdRow = CmdUpdate(sUser, conn2, cmdQuery);
				tran.Commit();				
				sRet[0] = "����I��";
				
			}
			catch (OracleException ex)
			{
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				tran.Rollback();
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				conn2 = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
			}
			
			return sRet;
		}
// ADD 2006.12.14 ���s�j�����J ���͐���S���폜 END

		/*********************************************************************
		 * �A�b�v���[�h�f�[�^�ǉ�
		 * �����F����b�c�A����b�c�A�׎�l�b�c...
		 * �ߒl�F�X�e�[�^�X
		 *
		 *********************************************************************/
		[WebMethod]
		public String[] Ins_uploadData(string[] sUser, string[] sList)
		{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//			logFileOpen(sUser);
			logWriter(sUser, INF, "�A�b�v���[�h�f�[�^�ǉ��J�n");

			OracleConnection conn2 = null;
			string[] sRet = new string[1];
//			string s�X�V���� = System.DateTime.Now.ToString("yyyyMMddHHmmss");

			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//// ADD 2005.05.23 ���s�j�ɉ� ����`�F�b�N�ǉ� START
//			// ����`�F�b�N
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//// ADD 2005.05.23 ���s�j�ɉ� ����`�F�b�N�ǉ� END

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			sRet[0] = "";
			try
			{
				for (int i = 0; i < sList.Length; i++)
				{
					string[] sData = sList[i].Split(',');
// ADD 2006.07.10 ���s�j���� �Z���b�c�̒ǉ� START
					string s�Z���b�c = " ";
					if(sData.Length >= 22 && sData[21].Length > 0){
						s�Z���b�c = sData[21];
					}
// ADD 2006.07.10 ���s�j���� �Z���b�c�̒ǉ� END
// ADD 2008.06.11 kcl)�X�{ ���X�R�[�h�������@�̕ύX START
					string s����b�c = " ";
//					if (sData.Length >=20 && sData[19].Length > 0) 
//					{
//						s����b�c = sData[19];
//					}
// ADD 2008.06.11 kcl)�X�{ ���X�R�[�h�������@�̕ύX END

					string cmdQuery = "";
					cmdQuery
						= "SELECT �폜�e�f \n"
						+   "FROM �r�l�O�Q�׎�l \n"
						+  "WHERE ����b�c = '" + sData[0] + "' \n"
						+    "AND ����b�c = '" + sData[1] + "' \n"
						+    "AND �׎�l�b�c = '" + sData[2] + "' "
						+    "FOR UPDATE ";

					OracleDataReader reader = CmdSelect(sUser, conn2, cmdQuery);
					int iCnt = 1;
					string s�폜�e�f = "";
					while (reader.Read())
					{
						s�폜�e�f = reader.GetString(0);
						iCnt++;
					}
// ADD 2005.06.08 ���s�j�ɉ� ORA-01000�΍� START
					reader.Close();
// ADD 2005.06.08 ���s�j�ɉ� ORA-01000�΍� END
					if(iCnt == 1)
					{
						//�ǉ�
						cmdQuery 
							= "INSERT INTO �r�l�O�Q�׎�l \n"
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
// MOD 2010.02.02 ���s�j���� �׎�l�}�X�^��[�o�^�o�f]�ɏo�׎g�p�����X�V START
//// MOD 2009.01.30 ���s�j���� [���O�R]�ɍŏI���p�N�����X�V START
////							+           "'" + sData[14] + "', "
//							+           "TO_CHAR(SYSDATE,'YYYYMM'), "
//// MOD 2009.01.30 ���s�j���� [���O�R]�ɍŏI���p�N�����X�V END
							+           "'" + sData[14] + "', "
// MOD 2010.02.02 ���s�j���� �׎�l�}�X�^��[�o�^�o�f]�ɏo�׎g�p�����X�V END
							+           "'" + sData[15] + "', \n"
// MOD 2006.07.10 ���s�j���� �Z���b�c�̒ǉ� START
//							+           "' ', "
							+           "'" + s�Z���b�c + "', "
// MOD 2006.07.10 ���s�j���� �Z���b�c�̒ǉ� END
							+           "'" + sData[16] + "', \n"
							+           "'" + sData[17] + "', "
// ADD 2008.06.11 kcl)�X�{ ���X�R�[�h�������@�̕ύX START
//							+           "' ', "
							+           "'" + s����b�c + "', \n"
// ADD 2008.06.11 kcl)�X�{ ���X�R�[�h�������@�̕ύX END
							+           "'" + sData[18] + "', \n"
							+           "' ', "
							+           "'0', "
							+           "TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), \n"
// MOD 2010.02.02 ���s�j���� �׎�l�}�X�^��[�o�^�o�f]�ɏo�׎g�p�����X�V START
//							+           "'���͎捞', "
							+           "' ', \n"
// MOD 2010.02.02 ���s�j���� �׎�l�}�X�^��[�o�^�o�f]�ɏo�׎g�p�����X�V END
							+           "'" + sData[20] + "', \n"
							+           "TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), "
							+           "'���͎捞', \n"
							+           "'" + sData[20] + "')";

						CmdUpdate(sUser, conn2, cmdQuery);

//�ۗ�
//						tran.Commit();
					}
					else
					{
						//�㏑���X�V
						cmdQuery
							= "UPDATE �r�l�O�Q�׎�l \n"
							+    "SET �d�b�ԍ��P = '" + sData[3] + "' "
							+       ",�d�b�ԍ��Q = '" + sData[4] + "' \n"
							+       ",�d�b�ԍ��R = '" + sData[5] + "' "
							+       ",�e�`�w�ԍ��P = '" + sData[6] + "' \n"
							+       ",�e�`�w�ԍ��Q = '" + sData[7] + "' "
							+       ",�e�`�w�ԍ��R = '" + sData[8] + "' \n"
							+       ",�Z���P = '" + sData[9] + "' "
							+       ",�Z���Q = '" + sData[10] + "' \n"
							+       ",�Z���R = '" + sData[11] + "' "
							+       ",���O�P = '" + sData[12] + "' \n"
							+       ",���O�Q = '" + sData[13] + "' "
// MOD 2010.02.02 ���s�j���� �׎�l�}�X�^��[�o�^�o�f]�ɏo�׎g�p�����X�V START
//// MOD 2009.01.30 ���s�j���� [���O�R]�ɍŏI���p�N�����X�V START
////							+       ",���O�R = '" + sData[14] + "' \n"
//							+       ",���O�R = TO_CHAR(SYSDATE,'YYYYMM') \n"
//// MOD 2009.01.30 ���s�j���� [���O�R]�ɍŏI���p�N�����X�V END
							+       ",���O�R = '" + sData[14] + "' \n"
// MOD 2010.02.02 ���s�j���� �׎�l�}�X�^��[�o�^�o�f]�ɏo�׎g�p�����X�V END
							+       ",�X�֔ԍ� = '" + sData[15] + "' "
// MOD 2006.07.10 ���s�j���� �Z���b�c�̒ǉ� START
//							+       ",�Z���b�c = ' ' \n"
							+       ",�Z���b�c = '" + s�Z���b�c + "' \n"
// MOD 2006.07.10 ���s�j���� �Z���b�c�̒ǉ� END
							+       ",�J�i���� = '" + sData[16] + "' "
							+       ",��ďo�׋敪 = '" + sData[17] + "' \n"
// ADD 2008.06.13 kcl)�X�{ ���X�R�[�h�������@�̕ύX START
//							+       ",����b�c = ' ' "
							+       ",����b�c = '" + s����b�c + "' \n"
// ADD 2008.06.13 kcl)�X�{ ���X�R�[�h�������@�̕ύX END
							+       ",����v = '" + sData[18] + "' \n"
							+       ",���[���A�h���X = ' ' "
// MOD 2005.06.29 ���s�j�����J �폜�e�f���O�̎��͓o�^�����͍X�V���Ȃ� START
//							+       ",�폜�e�f = '0' \n"
//							+       ",�o�^���� = " + s�X�V����
//							+       ",�o�^�o�f = '���͎捞' "
//							+       ",�o�^�� = '" + sData[20] + "' \n"
//							+       ",�X�V���� = " + s�X�V����
//							+       ",�X�V�o�f = '���͎捞' "
//							+       ",�X�V�� = '" + sData[20] + "' \n"
//							+ "WHERE ����b�c = '" + sData[0] + "' \n"
//							+   "AND ����b�c = '" + sData[1] + "' \n"
//							+   "AND �׎�l�b�c = '" + sData[2] + "' ";
							+       ",�폜�e�f = '0' \n"
// MOD 2010.02.02 ���s�j���� �׎�l�}�X�^��[�o�^�o�f]�ɏo�׎g�p�����X�V START
							+       ",�o�^�o�f = ' ' \n"
// MOD 2010.02.02 ���s�j���� �׎�l�}�X�^��[�o�^�o�f]�ɏo�׎g�p�����X�V END
							;
						if(s�폜�e�f == "1")
						{
							cmdQuery
								+=  ",�o�^���� = TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') "
// MOD 2010.02.02 ���s�j���� �׎�l�}�X�^��[�o�^�o�f]�ɏo�׎g�p�����X�V START
//								+   ",�o�^�o�f = '���͎捞' "
// MOD 2010.02.02 ���s�j���� �׎�l�}�X�^��[�o�^�o�f]�ɏo�׎g�p�����X�V END
								+   ",�o�^�� = '" + sData[20] + "' \n";
						}
						cmdQuery
							+=      ",�X�V���� = TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') "
							+       ",�X�V�o�f = '���͎捞' "
							+       ",�X�V�� = '" + sData[20] + "' \n"
							+ "WHERE ����b�c = '" + sData[0] + "' \n"
							+   "AND ����b�c = '" + sData[1] + "' \n"
							+   "AND �׎�l�b�c = '" + sData[2] + "' ";
// MOD 2005.06.29 ���s�j�����J �폜�e�f���O�̎��͓o�^�����͍X�V���Ȃ� END

							CmdUpdate(sUser, conn2, cmdQuery);
					}
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
					disposeReader(reader);
					reader = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END
				}
				logWriter(sUser, INF, "����I��");
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
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				conn2 = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
			}
			return sRet;
		}

// MOD 2010.03.11 ���s�j���� �b�r�u�捞���̗X�֔ԍ��P�O�O���ꊇ�`�F�b�N��ǉ� START
		/*********************************************************************
		 * �A�b�v���[�h�f�[�^�ǉ��Q
		 * �����F����b�c�A����b�c�A�׎�l�b�c...
		 * �ߒl�F�X�e�[�^�X
		 *
		 *********************************************************************/
		private static string INS_UPLOADDATA2_SELECT1
			= "SELECT 1 \n"
			+ " FROM �b�l�P�S�X�֔ԍ� \n"
			;

		[WebMethod]
		public String[] Ins_uploadData2(string[] sUser, string[] sList)
		{
			logWriter(sUser, INF, "�A�b�v���[�h�f�[�^�ǉ��Q�J�n");

			OracleConnection conn2 = null;
			string[] sRet = new string[sList.Length + 1];

			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null){
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

			OracleTransaction tran;
			tran = conn2.BeginTransaction();

			sRet[0] = "";
			try{
				for (int iRow = 0; iRow < sList.Length; iRow++){
					sRet[iRow+1] = "";

					string[] sData = sList[iRow].Split(',');
					string s�Z���b�c = " ";
					if(sData.Length >= 22 && sData[21].Length > 0){
						s�Z���b�c = sData[21];
					}
// ADD 2008.06.11 kcl)�X�{ ���X�R�[�h�������@�̕ύX START
					string s����b�c = " ";
//					if (sData.Length >=20 && sData[19].Length > 0) 
//					{
//						s����b�c = sData[19];
//					}
// ADD 2008.06.11 kcl)�X�{ ���X�R�[�h�������@�̕ύX END

//					sData[15] = sData[15].TrimEnd();
//					if(sData[15].Length == 0){
//						sRet[iRow+1] = "�X��";//���ݒ�
//						continue;
//					}
//					if(sData[15].Length != 7){
//						sRet[iRow+1] = "�X��";//�����Ɍ�肪����ꍇ
//						continue;
//					}

					//�X�֔ԍ��}�X�^�̑��݃`�F�b�N
					OracleDataReader reader;
					string cmdQuery = "";
					cmdQuery = INS_UPLOADDATA2_SELECT1
							+ "WHERE �X�֔ԍ� = '" + sData[15] + "' \n"
//�ۗ� MOD 2010.04.13 ���s�j���� �X�֔ԍ����폜���ꂽ���̏�Q�Ή� START
							+ "AND �폜�e�f = '0' \n"
//�ۗ� MOD 2010.04.13 ���s�j���� �X�֔ԍ����폜���ꂽ���̏�Q�Ή� END
							;

					reader = CmdSelect(sUser, conn2, cmdQuery);
					if(!reader.Read()){
						sRet[iRow+1] = sData[15];//�Y���f�[�^����
						reader.Close();
						disposeReader(reader);
						reader = null;
						continue;
					}
					reader.Close();

					cmdQuery
						= "SELECT �폜�e�f \n"
						+   "FROM �r�l�O�Q�׎�l \n"
						+  "WHERE ����b�c = '" + sData[0] + "' \n"
						+    "AND ����b�c = '" + sData[1] + "' \n"
						+    "AND �׎�l�b�c = '" + sData[2] + "' "
						+    "FOR UPDATE "
						;

					reader = CmdSelect(sUser, conn2, cmdQuery);
					int iCnt = 1;
					string s�폜�e�f = "";
					while (reader.Read()){
						s�폜�e�f = reader.GetString(0);
						iCnt++;
					}
					reader.Close();

					if(iCnt == 1){
						//�ǉ�
						cmdQuery 
							= "INSERT INTO �r�l�O�Q�׎�l \n"
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
							+           "'" + s�Z���b�c + "', "
							+           "'" + sData[16] + "', \n"
							+           "'" + sData[17] + "', "
// ADD 2008.06.11 kcl)�X�{ ���X�R�[�h�������@�̕ύX START
//							+           "' ', \n" //����b�c
							+           "'" + s����b�c + "', \n"
// ADD 2008.06.11 kcl)�X�{ ���X�R�[�h�������@�̕ύX END
							+           "'" + sData[18] + "', \n"
							+           "' ', "
							+           "'0', "
							+           "TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), \n"
							+           "' ', \n"
							+           "'" + sData[20] + "', \n"
							+           "TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), "
							+           "'���͎捞', \n"
							+           "'" + sData[20] + "')"
							;
						CmdUpdate(sUser, conn2, cmdQuery);
					}else{
						//�㏑���X�V
						cmdQuery
							= "UPDATE �r�l�O�Q�׎�l \n"
							+    "SET �d�b�ԍ��P = '" + sData[3] + "' "
							+       ",�d�b�ԍ��Q = '" + sData[4] + "' \n"
							+       ",�d�b�ԍ��R = '" + sData[5] + "' "
							+       ",�e�`�w�ԍ��P = '" + sData[6] + "' \n"
							+       ",�e�`�w�ԍ��Q = '" + sData[7] + "' "
							+       ",�e�`�w�ԍ��R = '" + sData[8] + "' \n"
							+       ",�Z���P = '" + sData[9] + "' "
							+       ",�Z���Q = '" + sData[10] + "' \n"
							+       ",�Z���R = '" + sData[11] + "' "
							+       ",���O�P = '" + sData[12] + "' \n"
							+       ",���O�Q = '" + sData[13] + "' "
							+       ",���O�R = '" + sData[14] + "' \n"
							+       ",�X�֔ԍ� = '" + sData[15] + "' "
							+       ",�Z���b�c = '" + s�Z���b�c + "' \n"
							+       ",�J�i���� = '" + sData[16] + "' "
							+       ",��ďo�׋敪 = '" + sData[17] + "' \n"
// ADD 2008.06.13 kcl)�X�{ ���X�R�[�h�������@�̕ύX START
//							+       ",����b�c = ' ' \n" //����b�c
							+       ",����b�c = '" + s����b�c + "' \n"
// ADD 2008.06.13 kcl)�X�{ ���X�R�[�h�������@�̕ύX END
							+       ",����v = '" + sData[18] + "' \n"
							+       ",���[���A�h���X = ' ' "
							+       ",�폜�e�f = '0' \n"
							+       ",�o�^�o�f = ' ' \n"
							;
						if(s�폜�e�f == "1"){
							cmdQuery
								+=  ",�o�^���� = TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') "
								+   ",�o�^�� = '" + sData[20] + "' \n"
								;
						}
						cmdQuery
							+=      ",�X�V���� = TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS') "
							+       ",�X�V�o�f = '���͎捞' "
							+       ",�X�V�� = '" + sData[20] + "' \n"
							+ "WHERE ����b�c = '" + sData[0] + "' \n"
							+   "AND ����b�c = '" + sData[1] + "' \n"
							+   "AND �׎�l�b�c = '" + sData[2] + "' "
							;

							CmdUpdate(sUser, conn2, cmdQuery);
					}
					disposeReader(reader);
					reader = null;
				}
				logWriter(sUser, INF, "����I��");
				tran.Commit();
			}catch (OracleException ex){
				tran.Rollback();
				sRet[0] = chgDBErrMsg(sUser, ex);
			}catch (Exception ex){
				tran.Rollback();
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}finally{
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			return sRet;
		}
// MOD 2010.03.11 ���s�j���� �b�r�u�捞���̗X�֔ԍ��P�O�O���ꊇ�`�F�b�N��ǉ� END

// ADD 2005.06.08 ���s�j�ɉ� �b�r�u�o�͒ǉ� START
		[WebMethod]
		public String[] Get_csvwrite(string[] sUser, string sKCode, string sBCode)
		{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//			logFileOpen(sUser);
			logWriter(sUser, INF, "�b�r�u�o�͗p�擾�J�n");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();

			string[] sRet = new string[1];
			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//
//			// ����`�F�b�N
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
				sbQuery.Append("SELECT �׎�l�b�c \n");
				sbQuery.Append(      ",�d�b�ԍ��P \n");
				sbQuery.Append(      ",�d�b�ԍ��Q \n");
				sbQuery.Append(      ",�d�b�ԍ��R \n");
				sbQuery.Append(      ",�e�`�w�ԍ��P \n");
				sbQuery.Append(      ",�e�`�w�ԍ��Q \n");
				sbQuery.Append(      ",�e�`�w�ԍ��R \n");
				sbQuery.Append(      ",�Z���P \n");
				sbQuery.Append(      ",�Z���Q \n");
				sbQuery.Append(      ",�Z���R \n");
				sbQuery.Append(      ",���O�P \n");
				sbQuery.Append(      ",���O�Q \n");
				sbQuery.Append(      ",' ' \n");
				sbQuery.Append(      ",�X�֔ԍ� \n");
				sbQuery.Append(      ",�J�i���� \n");
				sbQuery.Append(      ",��ďo�׋敪 \n");
				sbQuery.Append(      ",����v \n");
// MOD 2008.06.11 kcl)�X�{ ���X�R�[�h�������@�̕ύX START
//				sbQuery.Append(      ",' ' \n");
				sbQuery.Append(      ",����b�c \n");
// MOD 2008.06.11 kcl)�X�{ ���X�R�[�h�������@�̕ύX END
				sbQuery.Append( "FROM �r�l�O�Q�׎�l \n");
				sbQuery.Append("WHERE ����b�c = '" + sKCode + "' \n");
				sbQuery.Append(  "AND ����b�c = '" + sBCode + "' \n");
				sbQuery.Append(  "AND �폜�e�f = '0' \n");
// ADD 2009.01.29 ���s�j���� �ꗗ�̃\�[�g����[�׎�l�b�c]��ǉ� START
				sbQuery.Append("ORDER BY �׎�l�b�c \n");
// ADD 2009.01.29 ���s�j���� �ꗗ�̃\�[�g����[�׎�l�b�c]��ǉ� END

				OracleDataReader reader;
				reader = CmdSelect(sUser, conn2, sbQuery);

				StringBuilder sbData;
				while (reader.Read())
				{
					sbData = new StringBuilder(1024);
// MOD 2010.02.03 ���s�j���� �b�r�u�o�͎��ɕ������ڂ̑O��[']��ǉ� START
//					sbData.Append(sDbl + reader.GetString(0).Trim() + sDbl);			// �׎�l�b�c
//					sbData.Append(sKanma + sDbl + "(" + reader.GetString(1).Trim());	// �d�b�ԍ��P
//					sbData.Append(")" + reader.GetString(2).Trim() + "-");				// �d�b�ԍ��Q
//					sbData.Append(reader.GetString(3).Trim() + sDbl);					// �d�b�ԍ��R
//// MOD 2005.07.04 ���s�j���� �e�`�w�ԍ��̌`���G���[ START
////					sbData.Append(sKanma + sDbl + reader.GetString(4).Trim());			// �e�`�w�ԍ��P
////					sbData.Append(reader.GetString(5).Trim());							// �e�`�w�ԍ��Q
////					sbData.Append(reader.GetString(6).Trim() + sDbl);					// �e�`�w�ԍ��R
//					sbData.Append(sKanma + sDbl);
//					if(reader.GetString(4).Trim().Length > 0) 
//						sbData.Append("(" + reader.GetString(4).Trim() + ")");			// �e�`�w�ԍ��P
//					if(reader.GetString(5).Trim().Length > 0) 
//						sbData.Append(reader.GetString(5).Trim() + "-");				// �e�`�w�ԍ��Q
//					sbData.Append(reader.GetString(6).Trim() + sDbl);					// �e�`�w�ԍ��R
//// MOD 2005.07.04 ���s�j���� �e�`�w�ԍ��̌`���G���[ END
//					sbData.Append(sKanma + sDbl + reader.GetString(7).Trim() + sDbl);	// �Z���P
//					sbData.Append(sKanma + sDbl + reader.GetString(8).Trim() + sDbl);	// �Z���Q
//					sbData.Append(sKanma + sDbl + reader.GetString(9).Trim() + sDbl);	// �Z���R
//					sbData.Append(sKanma + sDbl + reader.GetString(10).Trim() + sDbl);	// ���O�P
//					sbData.Append(sKanma + sDbl + reader.GetString(11).Trim() + sDbl);	// ���O�Q
//					sbData.Append(sKanma + sDbl + reader.GetString(12).Trim() + sDbl);	// ���O�R
//					sbData.Append(sKanma + sDbl + reader.GetString(13).Trim() + sDbl);	// �X�֔ԍ�
//					sbData.Append(sKanma + sDbl + reader.GetString(14).Trim() + sDbl);	// �J�i����
//					sbData.Append(sKanma + sDbl + reader.GetString(15).Trim() + sDbl);	// ��ďo�׋敪
//					sbData.Append(sKanma + sDbl + reader.GetString(16).Trim() + sDbl);	// ����v
//					sbData.Append(sKanma + sDbl + reader.GetString(17).Trim() + sDbl);	// ���X�b�c
					sbData.Append(sDbl + sSng + reader.GetString(0).TrimEnd() + sDbl);	// �׎�l�b�c
					sbData.Append(sKanma + sDbl + sSng);
					if(reader.GetString(1).TrimEnd().Length > 0) 
						sbData.Append("(" + reader.GetString(1).TrimEnd() + ")");		// �d�b�ԍ��P
					if(reader.GetString(2).TrimEnd().Length > 0) 
						sbData.Append(reader.GetString(2).TrimEnd() + "-");				// �d�b�ԍ��Q
					sbData.Append(reader.GetString(3).TrimEnd() + sDbl);				// �d�b�ԍ��R
					sbData.Append(sKanma + sDbl + sSng);
					if(reader.GetString(4).TrimEnd().Length > 0) 
						sbData.Append("(" + reader.GetString(4).TrimEnd() + ")");		// �e�`�w�ԍ��P
					if(reader.GetString(5).TrimEnd().Length > 0) 
						sbData.Append(reader.GetString(5).TrimEnd() + "-");				// �e�`�w�ԍ��Q
					sbData.Append(reader.GetString(6).TrimEnd() + sDbl);				// �e�`�w�ԍ��R
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(7).TrimEnd() + sDbl);	// �Z���P
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(8).TrimEnd() + sDbl);	// �Z���Q
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(9).TrimEnd() + sDbl);	// �Z���R
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(10).TrimEnd() + sDbl);// ���O�P
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(11).TrimEnd() + sDbl);// ���O�Q
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(12).TrimEnd() + sDbl);// ���O�R
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(13).TrimEnd() + sDbl);// �X�֔ԍ�
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(14).TrimEnd() + sDbl);// �J�i����
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(15).TrimEnd() + sDbl);// ��ďo�׋敪
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(16).TrimEnd() + sDbl);// ����v
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(17).TrimEnd() + sDbl);// ���X�b�c
// MOD 2010.02.03 ���s�j���� �b�r�u�o�͎��ɕ������ڂ̑O��[']��ǉ� START
					sList.Add(sbData);
				}
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				disposeReader(reader);
				reader = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END

				sRet = new string[sList.Count + 1];
				if(sList.Count == 0) 
					sRet[0] = "�Y���f�[�^������܂���";
				else
				{
					sRet[0] = "����I��";
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
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				conn2 = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
			}
			return sRet;
		}
// ADD 2005.06.08 ���s�j�ɉ� �b�r�u�o�͒ǉ� END

// ADD 2006.07.03 ���s�j�R�{ �׎呍�����\�� START
		[WebMethod]
		public String[] Get_ninushiCount(string[] sUser, string sKCode, string sBCode)
		{
// MOD 2010.05.10 ���s�j���� �o�f��Q�̏C�� START
			return Get_otodokeCount(sUser, sKCode, sBCode);
		}

		[WebMethod]
		public String[] Get_otodokeCount(string[] sUser, string sKCode, string sBCode)
		{
// MOD 2010.05.10 ���s�j���� �o�f��Q�̏C�� END
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//			logFileOpen(sUser);
// MOD 2010.05.10 ���s�j���� �o�f��Q�̏C�� START
//			logWriter(sUser, INF, "�׎呍�����擾�J�n");
			logWriter(sUser, INF, "���͂��摍�����擾�J�n");
// MOD 2010.05.10 ���s�j���� �o�f��Q�̏C�� END

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[2];
			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//
//			// ����`�F�b�N
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
				sbQuery.Append("FROM �r�l�O�Q�׎�l  \n");
				sbQuery.Append(" WHERE ����b�c = '" + sKCode + "' \n");
				sbQuery.Append("   AND ����b�c = '" + sBCode + "' \n");
				sbQuery.Append(" AND �폜�e�f = '0' \n");

				OracleDataReader reader = CmdSelect(sUser, conn2, sbQuery);
// MOD 2010.05.10 ���s�j���� �o�f��Q�̏C�� START
//				reader.Read();
//				if (reader.GetDecimal(0) == 0)
//				{
//					sRet[0] = "�w�肳�ꂽ�׎�͑��݂��܂���";
//					sRet[1] = "0";
//				}
//				else
//				{
//					sRet[0] = "����I��";
//					sRet[1] = reader.GetDecimal(0).ToString().Trim();
//				}
				if(reader.Read()){
					sRet[0] = "����I��";
					sRet[1] = reader.GetDecimal(0).ToString().Trim();
				}else{
					sRet[0] = "���͂���}�X�^�ǂݍ��ݎ��ɃG���[���������܂���";
					sRet[1] = "0";
				}
// MOD 2010.05.10 ���s�j���� �o�f��Q�̏C�� END
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				disposeReader(reader);
				reader = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END
			}
			catch (OracleException ex)
			{
				sRet[0] = chgDBErrMsg(sUser, ex);
			}
			catch (Exception ex)
			{
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				conn2 = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
			}
			
			return sRet;
		}
// ADD 2006.07.03 ���s�j�R�{ �׎呍�����\�� END

// ADD 2006.07.05 ���s�j�R�{ �A�h���X����ʂ���̂b�r�u�o�͑Ή� START
// ADD 2007.02.14 FJCS�j�K�c ���������ɖ��O�̒ǉ� START
		[WebMethod]
		public String[] Get_csvwrite2(string[] sUser, string sKCode, string sBCode, string sKana, string sTCode, string sTelNo, string sTelNo2, string sTelNo3,string sName,int iSortLabel1,int iSortPat1,int iSortLabel2,int iSortPat2)
// ADD 2007.02.14 FJCS�j�K�c ���������ɖ��O�̒ǉ� END
// MOD 2010.02.03 ���s�j���� ���������ɍX�V����ǉ� START
		{
			return Get_csvwrite3(sUser, sKCode, sBCode, sKana, sTCode
								, sTelNo, sTelNo2, sTelNo3, sName
								, iSortLabel1, iSortPat1, iSortLabel2, iSortPat2
								, ""
								);
		}

		[WebMethod]
		public String[] Get_csvwrite3(string[] sUser, string sKCode, string sBCode, string sKana, string sTCode, string sTelNo, string sTelNo2, string sTelNo3,string sName,int iSortLabel1,int iSortPat1,int iSortLabel2,int iSortPat2, string sUpdateDay)
// MOD 2010.02.03 ���s�j���� ���������ɍX�V����ǉ� END
		{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//			logFileOpen(sUser);
			logWriter(sUser, INF, "�b�r�u�o�͗p�擾�J�n�R");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();

			string[] sRet = new string[1];
			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//
//			// ����`�F�b�N
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
				sbQuery.Append("SELECT �׎�l�b�c \n");
				sbQuery.Append(      ",�d�b�ԍ��P \n");
				sbQuery.Append(      ",�d�b�ԍ��Q \n");
				sbQuery.Append(      ",�d�b�ԍ��R \n");
				sbQuery.Append(      ",�e�`�w�ԍ��P \n");
				sbQuery.Append(      ",�e�`�w�ԍ��Q \n");
				sbQuery.Append(      ",�e�`�w�ԍ��R \n");
				sbQuery.Append(      ",�Z���P \n");
				sbQuery.Append(      ",�Z���Q \n");
				sbQuery.Append(      ",�Z���R \n");
				sbQuery.Append(      ",���O�P \n");
				sbQuery.Append(      ",���O�Q \n");
				sbQuery.Append(      ",' ' \n");
				sbQuery.Append(      ",�X�֔ԍ� \n");
				sbQuery.Append(      ",�J�i���� \n");
				sbQuery.Append(      ",��ďo�׋敪 \n");
				sbQuery.Append(      ",����v \n");
// MOD 2008.06.11 kcl)�X�{ ���X�R�[�h�������@�̕ύX START
//				sbQuery.Append(      ",' ' \n");
				sbQuery.Append(      ",����b�c \n");
// MOD 2008.06.11 kcl)�X�{ ���X�R�[�h�������@�̕ύX END
				sbQuery.Append( "FROM �r�l�O�Q�׎�l \n");
				sbQuery.Append("WHERE ����b�c = '" + sKCode + "' \n");
				sbQuery.Append(  "AND ����b�c = '" + sBCode + "' \n");
				if(sKana.Length > 0 && sTCode.Length == 0)
				{
					sbQuery.Append(" AND �J�i���� LIKE '%"+ sKana + "%' \n");
				}
				if(sTCode.Length > 0 && sKana.Length == 0)
				{
					sbQuery.Append(" AND �׎�l�b�c LIKE '"+ sTCode + "%' \n");
				}
				if(sTCode.Length > 0 && sKana.Length > 0)
				{
					sbQuery.Append(" AND �J�i���� LIKE '%"+ sKana + "%' \n");
					sbQuery.Append(" AND �׎�l�b�c LIKE '"+ sTCode + "%' \n");
				}
				if(sTelNo.Length > 0)
				{
					sbQuery.Append(" AND �d�b�ԍ��P LIKE '"+ sTelNo + "%' \n");
				}
				if(sTelNo2.Length > 0)
				{
					sbQuery.Append(" AND �d�b�ԍ��Q LIKE '"+ sTelNo2 + "%' \n");
				}
				if(sTelNo3.Length > 0)
				{
					sbQuery.Append(" AND �d�b�ԍ��R LIKE '"+ sTelNo3 + "%' \n");
				}
				// ADD 2007.01.30 FJCS�j�K�c ���������ɖ��O��ǉ� START
				if(sName.Length > 0)
				{
					sbQuery.Append(" AND ���O�P LIKE '%"+ sName + "%' \n");
				}
				// ADD 2007.01.30 FJCS�j�K�c ���������ɖ��O��ǉ� END
// MOD 2010.02.03 ���s�j���� ���������ɍX�V����ǉ� START
				if(sUpdateDay.Length > 0){
					string s�X�V�����r = sUpdateDay + "000000";
					string s�X�V�����d = sUpdateDay + "999999";
					sbQuery.Append(" AND �X�V���� BETWEEN "+s�X�V�����r+" AND "+s�X�V�����d+" \n");
				}
// MOD 2010.02.03 ���s�j���� ���������ɍX�V����ǉ� END

				sbQuery.Append(" AND �폜�e�f = '0' \n");
// MOD 2009.01.29 ���s�j���� �ꗗ�̃\�[�g����[�׎�l�b�c]��ǉ� START
//				if((iSortLabel1 != 0)||(iSortLabel2 != 0))
//					sbQuery.Append(" ORDER BY \n");
				sbQuery.Append(" ORDER BY \n");
// MOD 2009.01.29 ���s�j���� �ꗗ�̃\�[�g����[�׎�l�b�c]��ǉ� END
				if(iSortLabel1 != 0)
				{
					switch(iSortLabel1)
					{
// UPD 2007.02.14 FJCS�j�K�c Index���ڕύX START
//						case 1:
//							sbQuery.Append(" ���O�P ");
//							if(iSortPat1 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 2:
//							sbQuery.Append(" �׎�l�b�c ");
//							if(iSortPat1 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 3:
//							sbQuery.Append(" �d�b�ԍ��P ");
//							if(iSortPat1 == 1)
//								sbQuery.Append(" DESC \n");
//							sbQuery.Append(", �d�b�ԍ��Q ");
//							if(iSortPat1 == 1)
//								sbQuery.Append(" DESC \n");
//							sbQuery.Append(", �d�b�ԍ��R ");
//							if(iSortPat1 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 4:
//							sbQuery.Append(" �J�i���� ");
//							if(iSortPat1 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 5:
//							sbQuery.Append(" �o�^���� ");
//							if(iSortPat1 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 6:
//							sbQuery.Append(" �X�V����");
//							if(iSortPat1 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
						case 1:
							sbQuery.Append(" �J�i���� ");
							if(iSortPat1 == 1)
								sbQuery.Append(" DESC \n");
							break;
						case 2:
							sbQuery.Append(" �׎�l�b�c ");
							if(iSortPat1 == 1)
								sbQuery.Append(" DESC \n");
							break;
						case 3:
							sbQuery.Append(" �d�b�ԍ��P ");
							if(iSortPat1 == 1)
								sbQuery.Append(" DESC \n");
							sbQuery.Append(", �d�b�ԍ��Q ");
							if(iSortPat1 == 1)
								sbQuery.Append(" DESC \n");
							sbQuery.Append(", �d�b�ԍ��R ");
							if(iSortPat1 == 1)
								sbQuery.Append(" DESC \n");
							break;
						case 4:
							sbQuery.Append(" ���O�P ");
							if(iSortPat1 == 1)
								sbQuery.Append(" DESC \n");
// ADD 2009.01.29 ���s�j���� �ꗗ�̃\�[�g����[���O�Q]��ǉ� START
							sbQuery.Append(", ���O�Q ");
							if(iSortPat1 == 1)
								sbQuery.Append(" DESC \n");
// ADD 2009.01.29 ���s�j���� �ꗗ�̃\�[�g����[���O�Q]��ǉ� END
							break;
						case 5:
							sbQuery.Append(" �o�^���� ");
							if(iSortPat1 == 1)
								sbQuery.Append(" DESC \n");
							break;
						case 6:
							sbQuery.Append(" �X�V����");
							if(iSortPat1 == 1)
								sbQuery.Append(" DESC \n");
							break;
// UPD 2007.02.14 FJCS�j�K�c Index���ڕύX END
					}
					if(iSortLabel2 != 0)
						sbQuery.Append(" , ");
				}
				if(iSortLabel2 != 0)
				{
					switch(iSortLabel2)
					{
// UPD 2007.02.14 FJCS�j�K�c Index���ڕύX START
//						case 1:
//							sbQuery.Append(" ���O�P ");
//							if(iSortPat2 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 2:
//							sbQuery.Append(" �׎�l�b�c ");
//							if(iSortPat2 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 3:
//							sbQuery.Append(" �d�b�ԍ��P ");
//							if(iSortPat2 == 1)
//								sbQuery.Append(" DESC \n");
//							sbQuery.Append(", �d�b�ԍ��Q ");
//							if(iSortPat2 == 1)
//								sbQuery.Append(" DESC \n");
//							sbQuery.Append(", �d�b�ԍ��R ");
//							if(iSortPat2 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 4:
//							sbQuery.Append(" �J�i���� ");
//							if(iSortPat2 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 5:
//							sbQuery.Append(" �o�^���� ");
//							if(iSortPat2 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 6:
///							sbQuery.Append(" �X�V����");
//							if(iSortPat2 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
						case 1:
							sbQuery.Append(" �J�i���� ");
							if(iSortPat2 == 1)
								sbQuery.Append(" DESC \n");
							break;
						case 2:
							sbQuery.Append(" �׎�l�b�c ");
							if(iSortPat2 == 1)
								sbQuery.Append(" DESC \n");
							break;
						case 3:
							sbQuery.Append(" �d�b�ԍ��P ");
							if(iSortPat2 == 1)
								sbQuery.Append(" DESC \n");
							sbQuery.Append(", �d�b�ԍ��Q ");
							if(iSortPat2 == 1)
								sbQuery.Append(" DESC \n");
							sbQuery.Append(", �d�b�ԍ��R ");
							if(iSortPat2 == 1)
								sbQuery.Append(" DESC \n");
							break;
						case 4:
							sbQuery.Append(" ���O�P ");
							if(iSortPat2 == 1)
								sbQuery.Append(" DESC \n");
// ADD 2009.01.29 ���s�j���� �ꗗ�̃\�[�g����[���O�Q]��ǉ� START
							sbQuery.Append(", ���O�Q ");
							if(iSortPat2 == 1)
								sbQuery.Append(" DESC \n");
// ADD 2009.01.29 ���s�j���� �ꗗ�̃\�[�g����[���O�Q]��ǉ� END
							break;
						case 5:
							sbQuery.Append(" �o�^���� ");
							if(iSortPat2 == 1)
								sbQuery.Append(" DESC \n");
							break;
						case 6:
							sbQuery.Append(" �X�V����");
							if(iSortPat2 == 1)
								sbQuery.Append(" DESC \n");
							break;
// UPD 2007.02.14 FJCS�j�K�c Index���ڕύX END
					}
				}
// ADD 2009.01.29 ���s�j���� �ꗗ�̃\�[�g����[�׎�l�b�c]��ǉ� START
				if((iSortLabel1 != 0) || (iSortLabel2 != 0))
					sbQuery.Append(" , ");
				sbQuery.Append(" �׎�l�b�c \n");
// ADD 2009.01.29 ���s�j���� �ꗗ�̃\�[�g����[�׎�l�b�c]��ǉ� END

				OracleDataReader reader;
				reader = CmdSelect(sUser, conn2, sbQuery);

				StringBuilder sbData;
				while (reader.Read())
				{
					sbData = new StringBuilder(1024);
// MOD 2010.02.03 ���s�j���� �b�r�u�o�͎��ɕ������ڂ̑O��[']��ǉ� START
//					sbData.Append(sDbl + reader.GetString(0).Trim() + sDbl);			// �׎�l�b�c
//					sbData.Append(sKanma + sDbl + "(" + reader.GetString(1).Trim());	// �d�b�ԍ��P
//					sbData.Append(")" + reader.GetString(2).Trim() + "-");				// �d�b�ԍ��Q
//					sbData.Append(reader.GetString(3).Trim() + sDbl);					// �d�b�ԍ��R
//					sbData.Append(sKanma + sDbl);
//					if(reader.GetString(4).Trim().Length > 0) 
//						sbData.Append("(" + reader.GetString(4).Trim() + ")");			// �e�`�w�ԍ��P
//					if(reader.GetString(5).Trim().Length > 0) 
//						sbData.Append(reader.GetString(5).Trim() + "-");				// �e�`�w�ԍ��Q
//					sbData.Append(reader.GetString(6).Trim() + sDbl);					// �e�`�w�ԍ��R
//					sbData.Append(sKanma + sDbl + reader.GetString(7).Trim() + sDbl);	// �Z���P
//					sbData.Append(sKanma + sDbl + reader.GetString(8).Trim() + sDbl);	// �Z���Q
//					sbData.Append(sKanma + sDbl + reader.GetString(9).Trim() + sDbl);	// �Z���R
//					sbData.Append(sKanma + sDbl + reader.GetString(10).Trim() + sDbl);	// ���O�P
//					sbData.Append(sKanma + sDbl + reader.GetString(11).Trim() + sDbl);	// ���O�Q
//					sbData.Append(sKanma + sDbl + reader.GetString(12).Trim() + sDbl);	// ���O�R
//					sbData.Append(sKanma + sDbl + reader.GetString(13).Trim() + sDbl);	// �X�֔ԍ�
//					sbData.Append(sKanma + sDbl + reader.GetString(14).Trim() + sDbl);	// �J�i����
//					sbData.Append(sKanma + sDbl + reader.GetString(15).Trim() + sDbl);	// ��ďo�׋敪
//					sbData.Append(sKanma + sDbl + reader.GetString(16).Trim() + sDbl);	// ����v
//					sbData.Append(sKanma + sDbl + reader.GetString(17).Trim() + sDbl);	// ���X�b�c
					sbData.Append(sDbl + sSng + reader.GetString(0).TrimEnd() + sDbl);	// �׎�l�b�c
					sbData.Append(sKanma + sDbl + sSng);
					if(reader.GetString(1).TrimEnd().Length > 0) 
						sbData.Append("(" + reader.GetString(1).TrimEnd() + ")");		// �d�b�ԍ��P
					if(reader.GetString(2).TrimEnd().Length > 0) 
						sbData.Append(reader.GetString(2).TrimEnd() + "-");				// �d�b�ԍ��Q
					sbData.Append(reader.GetString(3).TrimEnd() + sDbl);				// �d�b�ԍ��R
					sbData.Append(sKanma + sDbl + sSng);
					if(reader.GetString(4).TrimEnd().Length > 0) 
						sbData.Append("(" + reader.GetString(4).TrimEnd() + ")");		// �e�`�w�ԍ��P
					if(reader.GetString(5).TrimEnd().Length > 0) 
						sbData.Append(reader.GetString(5).TrimEnd() + "-");				// �e�`�w�ԍ��Q
					sbData.Append(reader.GetString(6).TrimEnd() + sDbl);				// �e�`�w�ԍ��R
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(7).TrimEnd() + sDbl);	// �Z���P
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(8).TrimEnd() + sDbl);	// �Z���Q
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(9).TrimEnd() + sDbl);	// �Z���R
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(10).TrimEnd() + sDbl);// ���O�P
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(11).TrimEnd() + sDbl);// ���O�Q
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(12).TrimEnd() + sDbl);// ���O�R
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(13).TrimEnd() + sDbl);// �X�֔ԍ�
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(14).TrimEnd() + sDbl);// �J�i����
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(15).TrimEnd() + sDbl);// ��ďo�׋敪
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(16).TrimEnd() + sDbl);// ����v
					sbData.Append(sKanma + sDbl + sSng + reader.GetString(17).TrimEnd() + sDbl);// ���X�b�c
// MOD 2010.02.03 ���s�j���� �b�r�u�o�͎��ɕ������ڂ̑O��[']��ǉ� END
					sList.Add(sbData);
				}
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				disposeReader(reader);
				reader = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END

				sRet = new string[sList.Count + 1];
				if(sList.Count == 0) 
					sRet[0] = "�Y���f�[�^������܂���";
				else
				{
					sRet[0] = "����I��";
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
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				conn2 = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
			}
			return sRet;
		}
// ADD 2006.07.05 ���s�j�R�{ �A�h���X����ʂ���̂b�r�u�o�͑Ή� END
// ADD 2006.07.10 ���s�j�R�{ ���������ɓd�b�ԍ����\�[�g�����̒ǉ� START
// ADD 2007.01.30 FJCS�j�K�c ���������ɖ��O�̒ǉ� START
		[WebMethod]
		public String[] Get_todokesaki2(string[] sUser, string sKCode, string sBCode, string sKana, string sTCode, string sTelNo, string sTelNo2, string sTelNo3,string sName, int iSortLabel1,int iSortPat1,int iSortLabel2,int iSortPat2)
// ADD 2007.01.30 FJCS�j�K�c ���������ɖ��O�̒ǉ� END
		{
// ADD 2009.01.29 ���s�j���� �ꗗ�ɖ��O�Q�A�Z���Q�A�Z���R��ǉ� START
			return Get_todokesaki3(sUser, sKCode, sBCode, sKana, sTCode, sTelNo, sTelNo2, sTelNo3
									, sName, iSortLabel1, iSortPat1, iSortLabel2, iSortPat2, false);
		}

		[WebMethod]
		public String[] Get_todokesaki3(string[] sUser, string sKCode, string sBCode, string sKana, string sTCode, string sTelNo, string sTelNo2, string sTelNo3,string sName, int iSortLabel1,int iSortPat1,int iSortLabel2,int iSortPat2, bool bMultiLine)
// ADD 2007.01.30 FJCS�j�K�c ���������ɖ��O�̒ǉ� END
// MOD 2010.02.03 ���s�j���� ���������ɍX�V����ǉ� START
		{
			return Get_todokesaki4(sUser, sKCode, sBCode, sKana, sTCode, sTelNo, sTelNo2, sTelNo3
									, sName, iSortLabel1, iSortPat1, iSortLabel2, iSortPat2, false, "");
		}

		[WebMethod]
		public String[] Get_todokesaki4(string[] sUser, string sKCode, string sBCode, string sKana, string sTCode, string sTelNo, string sTelNo2, string sTelNo3,string sName, int iSortLabel1,int iSortPat1,int iSortLabel2,int iSortPat2, bool bMultiLine, string sUpdateDay)
// MOD 2010.02.03 ���s�j���� ���������ɍX�V����ǉ� END
		{
// ADD 2009.01.29 ���s�j���� �ꗗ�ɖ��O�Q�A�Z���Q�A�Z���R��ǉ� END
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//			logFileOpen(sUser);
			logWriter(sUser, INF, "�͂���ꗗ�擾�J�n");

			OracleConnection conn2 = null;
			ArrayList sList = new ArrayList();
			string[] sRet = new string[1];
			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if(conn2 == null)
			{
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//// ADD 2005.05.23 ���s�j�ɉ� ����`�F�b�N�ǉ� START
//			// ����`�F�b�N
//			sRet[0] = userCheck2(conn2, sUser);
//			if(sRet[0].Length > 0)
//			{
//				disconnect2(sUser, conn2);
//				logFileClose();
//				return sRet;
//			}
//// ADD 2005.05.23 ���s�j�ɉ� ����`�F�b�N�ǉ� END

			StringBuilder sbQuery = new StringBuilder(1024);
			StringBuilder sbRet = new StringBuilder(1024);
			try
			{
				sbQuery.Append(GET_OTODOKE_SELECT);
				sbQuery.Append(" WHERE ����b�c = '" + sKCode + "' \n");
				sbQuery.Append("   AND ����b�c = '" + sBCode + "' \n");
				if(sKana.Length > 0 && sTCode.Length == 0)
				{
					sbQuery.Append(" AND �J�i���� LIKE '%"+ sKana + "%' \n");
				}
				if(sTCode.Length > 0 && sKana.Length == 0)
				{
					sbQuery.Append(" AND �׎�l�b�c LIKE '"+ sTCode + "%' \n");
				}
				if(sTCode.Length > 0 && sKana.Length > 0)
				{
					sbQuery.Append(" AND �J�i���� LIKE '%"+ sKana + "%' \n");
					sbQuery.Append(" AND �׎�l�b�c LIKE '"+ sTCode + "%' \n");
				}
				// ADD 2006.07.04 ���s�j�R�{ ���������ɓd�b�ԍ���ǉ� START
				if(sTelNo.Length > 0)
				{
					sbQuery.Append(" AND �d�b�ԍ��P LIKE '"+ sTelNo + "%' \n");
				}
				if(sTelNo2.Length > 0)
				{
					sbQuery.Append(" AND �d�b�ԍ��Q LIKE '"+ sTelNo2 + "%' \n");
				}
				if(sTelNo3.Length > 0)
				{
					sbQuery.Append(" AND �d�b�ԍ��R LIKE '"+ sTelNo3 + "%' \n");
				}
				// ADD 2006.07.04 ���s�j�R�{ ���������ɓd�b�ԍ���ǉ� END
				// ADD 2007.01.30 FJCS�j�K�c ���������ɖ��O��ǉ� START
				if(sName.Length > 0)
				{
					sbQuery.Append(" AND ���O�P LIKE '%"+ sName + "%' \n");
				}
				// ADD 2007.01.30 FJCS�j�K�c ���������ɖ��O��ǉ� END
// MOD 2010.02.03 ���s�j���� ���������ɍX�V����ǉ� START
				if(sUpdateDay.Length > 0){
					string s�X�V�����r = sUpdateDay + "000000";
					string s�X�V�����d = sUpdateDay + "999999";
					sbQuery.Append(" AND �X�V���� BETWEEN "+s�X�V�����r+" AND "+s�X�V�����d+" \n");
				}
// MOD 2010.02.03 ���s�j���� ���������ɍX�V����ǉ� END

				sbQuery.Append(" AND �폜�e�f = '0' \n");
				// MOD 2006.07.04 ���s�j�R�{ ���������Ƀ\�[�g�@�\��ǉ� START
				//				sbQuery.Append(" ORDER BY ���O�P \n");
// MOD 2009.01.29 ���s�j���� �ꗗ�̃\�[�g����[�׎�l�b�c]��ǉ� START
//				if((iSortLabel1 != 0)||(iSortLabel2 != 0))
//					sbQuery.Append(" ORDER BY \n");
				sbQuery.Append(" ORDER BY \n");
// MOD 2009.01.29 ���s�j���� �ꗗ�̃\�[�g����[�׎�l�b�c]��ǉ� END
				if(iSortLabel1 != 0)
				{
					switch(iSortLabel1)
					{
// UPD 2007.01.30 FJCS�j�K�c Index���ڕύX START
//						case 1:
//							sbQuery.Append(" ���O�P ");
//							if(iSortPat1 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 2:
//							sbQuery.Append(" �׎�l�b�c ");
//							if(iSortPat1 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 3:
//							sbQuery.Append(" �d�b�ԍ��P ");
//							if(iSortPat1 == 1)
//								sbQuery.Append(" DESC \n");
//							sbQuery.Append(", �d�b�ԍ��Q ");
//							if(iSortPat1 == 1)
//								sbQuery.Append(" DESC \n");
//							sbQuery.Append(", �d�b�ԍ��R ");
//							if(iSortPat1 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 4:
//							sbQuery.Append(" �J�i���� ");
//							if(iSortPat1 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 5:
//							sbQuery.Append(" �o�^���� ");
//							if(iSortPat1 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 6:
//							sbQuery.Append(" �X�V����");
//							if(iSortPat1 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
						case 1:
							sbQuery.Append(" �J�i���� ");
							if(iSortPat1 == 1)
								sbQuery.Append(" DESC \n");
							break;
						case 2:
							sbQuery.Append(" �׎�l�b�c ");
							if(iSortPat1 == 1)
								sbQuery.Append(" DESC \n");
							break;
						case 3:
							sbQuery.Append(" �d�b�ԍ��P ");
							if(iSortPat1 == 1)
								sbQuery.Append(" DESC \n");
							sbQuery.Append(", �d�b�ԍ��Q ");
							if(iSortPat1 == 1)
								sbQuery.Append(" DESC \n");
							sbQuery.Append(", �d�b�ԍ��R ");
							if(iSortPat1 == 1)
								sbQuery.Append(" DESC \n");
							break;
						case 4:
							sbQuery.Append(" ���O�P ");
							if(iSortPat1 == 1)
								sbQuery.Append(" DESC \n");
// ADD 2009.01.29 ���s�j���� �ꗗ�̃\�[�g����[���O�Q]��ǉ� START
							sbQuery.Append(", ���O�Q ");
							if(iSortPat1 == 1)
								sbQuery.Append(" DESC \n");
// ADD 2009.01.29 ���s�j���� �ꗗ�̃\�[�g����[���O�Q]��ǉ� END
							break;
						case 5:
							sbQuery.Append(" �o�^���� ");
							if(iSortPat1 == 1)
								sbQuery.Append(" DESC \n");
							break;
						case 6:
							sbQuery.Append(" �X�V����");
							if(iSortPat1 == 1)
								sbQuery.Append(" DESC \n");
							break;
// UPD 2007.01.30 FJCS�j�K�c Index���ڕύX END
					}
					if(iSortLabel2 != 0)
						sbQuery.Append(" , ");
				}
				if(iSortLabel2 != 0)
				{
					switch(iSortLabel2)
					{
// UPD 2007.01.30 FJCS�j�K�c Index���ڕύX START
//						case 1:
//							sbQuery.Append(" ���O�P ");
//							if(iSortPat2 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 2:
//							sbQuery.Append(" �׎�l�b�c ");
//							if(iSortPat2 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 3:
//							sbQuery.Append(" �d�b�ԍ��P ");
//							if(iSortPat2 == 1)
//								sbQuery.Append(" DESC \n");
//							sbQuery.Append(", �d�b�ԍ��Q ");
//							if(iSortPat2 == 1)
//								sbQuery.Append(" DESC \n");
//							sbQuery.Append(", �d�b�ԍ��R ");
//							if(iSortPat2 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 4:
//							sbQuery.Append(" �J�i���� ");
//							if(iSortPat2 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 5:
//							sbQuery.Append(" �o�^���� ");
//							if(iSortPat2 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
//						case 6:
//							sbQuery.Append(" �X�V����");
//							if(iSortPat2 == 1)
//								sbQuery.Append(" DESC \n");
//							break;
						case 1:
							sbQuery.Append(" �J�i���� ");
							if(iSortPat2 == 1)
								sbQuery.Append(" DESC \n");
							break;
						case 2:
							sbQuery.Append(" �׎�l�b�c ");
							if(iSortPat2 == 1)
								sbQuery.Append(" DESC \n");
							break;
						case 3:
							sbQuery.Append(" �d�b�ԍ��P ");
							if(iSortPat2 == 1)
								sbQuery.Append(" DESC \n");
							sbQuery.Append(", �d�b�ԍ��Q ");
							if(iSortPat2 == 1)
								sbQuery.Append(" DESC \n");
							sbQuery.Append(", �d�b�ԍ��R ");
							if(iSortPat2 == 1)
								sbQuery.Append(" DESC \n");
							break;
						case 4:
							sbQuery.Append(" ���O�P ");
							if(iSortPat2 == 1)
								sbQuery.Append(" DESC \n");
// ADD 2009.01.29 ���s�j���� �ꗗ�ɖ��O�Q�A�Z���Q�A�Z���R��ǉ� START
							sbQuery.Append(", ���O�Q ");
							if(iSortPat2 == 1)
								sbQuery.Append(" DESC \n");
// ADD 2009.01.29 ���s�j���� �ꗗ�ɖ��O�Q�A�Z���Q�A�Z���R��ǉ� END
							break;
						case 5:
							sbQuery.Append(" �o�^���� ");
							if(iSortPat2 == 1)
								sbQuery.Append(" DESC \n");
							break;
						case 6:
							sbQuery.Append(" �X�V����");
							if(iSortPat2 == 1)
								sbQuery.Append(" DESC \n");
							break;
// UPD 2007.01.30 FJCS�j�K�c Index���ڕύX END
					}
				}
				// MOD 2006.07.04 ���s�j�R�{ ���������Ƀ\�[�g�@�\��ǉ� END
// ADD 2009.01.29 ���s�j���� �ꗗ�̃\�[�g����[�׎�l�b�c]��ǉ� START
				if((iSortLabel1 != 0) || (iSortLabel2 != 0))
					sbQuery.Append(" , ");
				sbQuery.Append(" �׎�l�b�c \n");
// ADD 2009.01.29 ���s�j���� �ꗗ�̃\�[�g����[�׎�l�b�c]��ǉ� END

				OracleDataReader reader = CmdSelect(sUser, conn2, sbQuery);
				while (reader.Read())
				{
					sbRet = new StringBuilder(1024);

// MOD 2011.01.18 ���s�j���� �Z�����O�̑OSPACE���߂Ȃ� START
//					sbRet.Append(sSepa + reader.GetString(0).Trim());
					sbRet.Append(sSepa + reader.GetString(0).TrimEnd()); // ���O�P
// MOD 2011.01.18 ���s�j���� �Z�����O�̑OSPACE���߂Ȃ� END
// ADD 2009.01.29 ���s�j���� �ꗗ�ɖ��O�Q�A�Z���Q�A�Z���R��ǉ� START
					if(bMultiLine){
// MOD 2011.01.18 ���s�j���� �Z�����O�̑OSPACE���߂Ȃ� START
//						sbRet.Append(sCRLF + reader.GetString(7).Trim()); // ���O�Q
						sbRet.Append(sCRLF + reader.GetString(7).TrimEnd()); // ���O�Q
// MOD 2011.01.18 ���s�j���� �Z�����O�̑OSPACE���߂Ȃ� END
					}
// ADD 2009.01.29 ���s�j���� �ꗗ�ɖ��O�Q�A�Z���Q�A�Z���R��ǉ� END
// MOD 2011.01.18 ���s�j���� �Z�����O�̑OSPACE���߂Ȃ� START
//					sbRet.Append(sSepa + reader.GetString(1).Trim());
					sbRet.Append(sSepa + reader.GetString(1).TrimEnd()); // �Z���P
// MOD 2011.01.18 ���s�j���� �Z�����O�̑OSPACE���߂Ȃ� END
// ADD 2009.01.29 ���s�j���� �ꗗ�ɖ��O�Q�A�Z���Q�A�Z���R��ǉ� START
					if(bMultiLine){
// MOD 2011.01.18 ���s�j���� �Z�����O�̑OSPACE���߂Ȃ� START
//						sbRet.Append(sCRLF + reader.GetString(8).Trim()); // �Z���Q
//						sbRet.Append(sCRLF + reader.GetString(9).Trim()); // �Z���R
						sbRet.Append(sCRLF + reader.GetString(8).TrimEnd()); // �Z���Q
						sbRet.Append(sCRLF + reader.GetString(9).TrimEnd()); // �Z���R
// MOD 2011.01.18 ���s�j���� �Z�����O�̑OSPACE���߂Ȃ� END
					}
// ADD 2009.01.29 ���s�j���� �ꗗ�ɖ��O�Q�A�Z���Q�A�Z���R��ǉ� END
					sbRet.Append(sSepa + reader.GetString(2).Trim());
// MOD 2005.05.11 ���s�j���� ORA-03113�΍�H START
//					sbRet.Append(sSepa + reader.GetString(3));
//					sbRet.Append(sSepa + reader.GetString(4).Trim());
					sbRet.Append(sSepa + "(" + reader.GetString(3).Trim() + ")"
						+ reader.GetString(4).Trim() + "-"
						+ reader.GetString(5).Trim());	// �d�b�ԍ�
// MOD 2011.01.18 ���s�j���� �Z�����O�̑OSPACE���߂Ȃ� START
//					sbRet.Append(sSepa + reader.GetString(6).Trim());	// �J�i����
					sbRet.Append(sSepa + reader.GetString(6).TrimEnd());// �J�i����
// MOD 2011.01.18 ���s�j���� �Z�����O�̑OSPACE���߂Ȃ� END
// MOD 2005.05.11 ���s�j���� ORA-03113�΍�H END
// MOD 2010.02.02 ���s�j���� �ꗗ�ɓo�^�����A�X�V�����A�o�׎g�p����ǉ� START
					sbRet.Append(sSepa + reader.GetString(10).TrimEnd());	//�X�֔ԍ�
					sbRet.Append(sSepa + reader.GetString(11).TrimEnd());	//����v
					sbRet.Append(sSepa + reader.GetString(12).TrimEnd());	//���[���A�h���X
					sbRet.Append(sSepa + reader.GetString(13).TrimEnd());	//�Z���b�c
					sbRet.Append(sSepa + reader.GetString(14).TrimEnd());	//���X�b�c�i����b�c�j
					sbRet.Append(sSepa + ToYYYYMMDD(reader.GetString(15)));	//�o�^����
					sbRet.Append(sSepa + ToYYYYMMDD(reader.GetString(16)));	//�X�V����
					sbRet.Append(sSepa + ToYYYYMMDD(reader.GetString(17)));	//�o�׎g�p��
// MOD 2010.02.02 ���s�j���� �ꗗ�ɓo�^�����A�X�V�����A�o�׎g�p����ǉ� END

					sList.Add(sbRet);
				}
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				disposeReader(reader);
				reader = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END

				sRet = new string[sList.Count + 1];
				if(sList.Count == 0) 
					sRet[0] = "�Y���f�[�^������܂���";
				else
				{
					sRet[0] = "����I��";
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
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				disconnect2(sUser, conn2);
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� START
				conn2 = null;
// ADD 2007.04.28 ���s�j���� �I�u�W�F�N�g�̔j�� END
// DEL 2007.05.10 ���s�j���� ���g�p�֐��̃R�����g��
//				logFileClose();
			}
			
			return sRet;
		}
// ADD 2006.07.10 ���s�j�R�{ ���������ɓd�b�ԍ����\�[�g�����̒ǉ� END

// ADD 2008.10.16 kcl)�X�{ �X���R�[�h���݃`�F�b�N�ǉ� START
		/*********************************************************************
		 * �X���R�[�h���݃`�F�b�N
		 * �����F�X���b�c
		 * �ߒl�F�X�e�[�^�X
		 *********************************************************************/
		[WebMethod]
		public string [] Check_TensyoCode(string [] sUser, string sTensyoCode)
		{
			// ���O�L�^
			logWriter(sUser, INF, "�X���R�[�h���݊m�F�J�n");

			OracleConnection conn2 = null;
			OracleDataReader reader = null;
			string cmdQuery;
			string [] sRet = new string[1];

			// �c�a�ڑ�
			conn2 = connect2(sUser);
			if (conn2 == null)
			{
				// �c�a�ڑ��G���[
				sRet[0] = "�c�a�ڑ��G���[";
				return sRet;
			}

			try
			{
				// SQL��
				cmdQuery 
					= "SELECT Count(*) "
					+ "  FROM �b�l�P�O�X�� CM10 \n"
					+ " WHERE CM10.�X���b�c = '" + sTensyoCode + "' \n"
					+ "   AND CM10.�폜�e�f = '0' \n"
					;

				// SQL���s
				reader = CmdSelect(sUser, conn2, cmdQuery);

				// ���s���ʎ擾
				if (reader.Read())
				{
					// �ǂݎ�萬��
					decimal cnt = reader.GetDecimal(0);
					if (cnt > 0m)
					{
						// �X���}�X�^�ɓo�^����
						sRet[0] = "����I��";
					} 
					else 
					{
						// �X���}�X�^�ɓo�^�Ȃ�
						sRet[0] = "�X���R�[�h���X���}�X�^�ɓo�^����Ă��܂���B";
					}
				}
				else
				{
					// �ǂݎ�莸�s
					sRet[0] = "�X���}�X�^�ǂݍ��ݎ��ɃG���[���������܂����B";
				}
			}
			catch (OracleException ex)
			{
				// Oracle�̃G���[
				sRet[0] = chgDBErrMsg(sUser, ex);
				// ���O�L�^
				logWriter(sUser, ERR, sRet[0]);
			}
			catch (Exception ex)
			{
				// ���̑��̃G���[
				sRet[0] = "�T�[�o�G���[�F" + ex.Message;
				// ���O�L�^
				logWriter(sUser, ERR, sRet[0]);
			}
			finally
			{
				// reader�̏I������
				if (reader != null) 
				{
					disposeReader(reader);
					reader = null;
				}

				// �c�a�ؒf
				disconnect2(sUser, conn2);
				conn2 = null;
			}
			
			return sRet;
		}
// ADD 2008.10.16 kcl)�X�{ �X���R�[�h���݃`�F�b�N�ǉ� END
// MOD 2010.02.02 ���s�j���� �ꗗ�ɓo�^�����A�X�V�����A�o�׎g�p����ǉ� START
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
// MOD 2010.02.02 ���s�j���� �ꗗ�ɓo�^�����A�X�V�����A�o�׎g�p����ǉ� END
	}
}

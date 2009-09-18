/*
 * Copyright (C) 2008, Google Inc.
 *
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or
 * without modification, are permitted provided that the following
 * conditions are met:
 *
 * - Redistributions of source code must retain the above copyright
 *   notice, this list of conditions and the following disclaimer.
 *
 * - Redistributions in binary form must reproduce the above
 *   copyright notice, this list of conditions and the following
 *   disclaimer in the documentation and/or other materials provided
 *   with the distribution.
 *
 * - Neither the name of the Git Development Community nor the
 *   names of its contributors may be used to endorse or promote
 *   products derived from this software without specific prior
 *   written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND
 * CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
 * CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
 * STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System.IO;
using System.Text;
using GitSharp.RevWalk;
using GitSharp.Tests.Util;
using Xunit;

namespace GitSharp.Tests.RevWalk
{
	public class RevCommitParseTest : RepositoryTestCase
	{
		private readonly Encoding _utf8Enc = Constants.CHARSET;
		private readonly Encoding _isoEnc = Encoding.GetEncoding("ISO-8859-1");
		private readonly Encoding _eucJpEnc = Encoding.GetEncoding("EUC-JP");

		[StrictFactAttribute]
		public void testParse_NoParents()
		{
			ObjectId treeId = Id("9788669ad918b6fcce64af8882fc9a81cb6aba67");
			const string authorName = "A U. Thor";
			const string authorEmail = "a_u_thor@example.com";
			const int authorTime = 1218123387;

			const string committerName = "C O. Miter";
			const string committerEmail = "comiter@example.com";
			const int committerTime = 1218123390;
			var body = new StringBuilder();

			body.Append("tree ");
			body.Append(treeId.Name);
			body.Append("\n");

			body.Append("author ");
			body.Append(authorName);
			body.Append(" <");
			body.Append(authorEmail);
			body.Append("> ");
			body.Append(authorTime);
			body.Append(" +0700\n");

			body.Append("committer ");
			body.Append(committerName);
			body.Append(" <");
			body.Append(committerEmail);
			body.Append("> ");
			body.Append(committerTime);
			body.Append(" -0500\n");

			body.Append("\n");

			var rw = new GitSharp.RevWalk.RevWalk(db);

			var c = new RevCommit(Id("9473095c4cb2f12aefe1db8a355fe3fafba42f67"));
			Assert.Null(c.Tree);
			Assert.Null(c.Parents);

			c.parseCanonical(rw, _utf8Enc.GetBytes(body.ToString()));
			Assert.NotNull(c.Tree);
			Assert.Equal(treeId, c.Tree.getId());
			Assert.Same(rw.lookupTree(treeId), c.Tree);

			Assert.NotNull(c.Parents);
			Assert.Equal(0, c.Parents.Length);
			Assert.Equal(string.Empty, c.getFullMessage());

			PersonIdent cAuthor = c.getAuthorIdent();
			Assert.NotNull(cAuthor);
			Assert.Equal(authorName, cAuthor.Name);
			Assert.Equal(authorEmail, cAuthor.EmailAddress);

			PersonIdent cCommitter = c.getCommitterIdent();
			Assert.NotNull(cCommitter);
			Assert.Equal(committerName, cCommitter.Name);
			Assert.Equal(committerEmail, cCommitter.EmailAddress);
		}

		private RevCommit Create(string msg)
		{
			var b = new StringBuilder();
			b.Append("tree 9788669ad918b6fcce64af8882fc9a81cb6aba67\n");
			b.Append("author A U. Thor <a_u_thor@example.com> 1218123387 +0700\n");
			b.Append("committer C O. Miter <c@example.com> 1218123390 -0500\n");
			b.Append("\n");
			b.Append(msg);

			var c = new RevCommit(Id("9473095c4cb2f12aefe1db8a355fe3fafba42f67"));

			c.parseCanonical(new GitSharp.RevWalk.RevWalk(db), _utf8Enc.GetBytes(b.ToString()));
			return c;
		}

		[StrictFactAttribute]
		public void testParse_WeirdHeaderOnlyCommit()
		{
			var b = new StringBuilder();
			b.Append("tree 9788669ad918b6fcce64af8882fc9a81cb6aba67\n");
			b.Append("author A U. Thor <a_u_thor@example.com> 1218123387 +0700\n");
			b.Append("committer C O. Miter <c@example.com> 1218123390 -0500\n");

			var c = new RevCommit(Id("9473095c4cb2f12aefe1db8a355fe3fafba42f67"));

			c.parseCanonical(new GitSharp.RevWalk.RevWalk(db), _utf8Enc.GetBytes(b.ToString()));

			Assert.Equal(string.Empty, c.getFullMessage());
			Assert.Equal(string.Empty, c.getShortMessage());
		}

		[StrictFactAttribute]
		public void testParse_implicit_UTF8_encoded()
		{
			RevCommit c;
			using (var b = new BinaryWriter(new MemoryStream()))
			{
				b.Write(_utf8Enc.GetBytes("tree 9788669ad918b6fcce64af8882fc9a81cb6aba67\n"));
				b.Write(_utf8Enc.GetBytes("author F\u00f6r fattare <a_u_thor@example.com> 1218123387 +0700\n"));
				b.Write(_utf8Enc.GetBytes("committer C O. Miter <c@example.com> 1218123390 -0500\n"));
				b.Write(_utf8Enc.GetBytes("\n"));
				b.Write(_utf8Enc.GetBytes("Sm\u00f6rg\u00e5sbord\n"));
				b.Write(_utf8Enc.GetBytes("\n"));
				b.Write(_utf8Enc.GetBytes("\u304d\u308c\u3044\n"));
				c = new RevCommit(Id("9473095c4cb2f12aefe1db8a355fe3fafba42f67")); // bogus id
				c.parseCanonical(new GitSharp.RevWalk.RevWalk(db), ((MemoryStream)b.BaseStream).ToArray());
			}

			Assert.Equal("F\u00f6r fattare", c.getAuthorIdent().Name);
			Assert.Equal("Sm\u00f6rg\u00e5sbord", c.getShortMessage());
			Assert.Equal("Sm\u00f6rg\u00e5sbord\n\n\u304d\u308c\u3044\n", c.getFullMessage());
		}

		[StrictFactAttribute]
		public void testParse_implicit_mixed_encoded()
		{
			RevCommit c;
			using (var b = new BinaryWriter(new MemoryStream()))
			{
				b.Write(_utf8Enc.GetBytes("tree 9788669ad918b6fcce64af8882fc9a81cb6aba67\n"));
				b.Write(_isoEnc.GetBytes("author F\u00f6r fattare <a_u_thor@example.com> 1218123387 +0700\n"));
				b.Write(_utf8Enc.GetBytes("committer C O. Miter <c@example.com> 1218123390 -0500\n"));
				b.Write(_utf8Enc.GetBytes("\n"));
				b.Write(_utf8Enc.GetBytes("Sm\u00f6rg\u00e5sbord\n"));
				b.Write(_utf8Enc.GetBytes("\n"));
				b.Write(_utf8Enc.GetBytes("\u304d\u308c\u3044\n"));

				c = new RevCommit(Id("9473095c4cb2f12aefe1db8a355fe3fafba42f67")); // bogus id
				c.parseCanonical(new GitSharp.RevWalk.RevWalk(db), ((MemoryStream)b.BaseStream).ToArray());
			}

			Assert.Equal("F\u00f6r fattare", c.getAuthorIdent().Name);
			Assert.Equal("Sm\u00f6rg\u00e5sbord", c.getShortMessage());
			Assert.Equal("Sm\u00f6rg\u00e5sbord\n\n\u304d\u308c\u3044\n", c.getFullMessage());
		}

		/// <summary>
		/// Test parsing of a commit whose encoding is given and works.
		/// </summary>
		[StrictFactAttribute]
		public void testParse_explicit_encoded()
		{
			RevCommit c;
			using (var b = new BinaryWriter(new MemoryStream()))
			{
				b.Write(_eucJpEnc.GetBytes("tree 9788669ad918b6fcce64af8882fc9a81cb6aba67\n"));
				b.Write(_eucJpEnc.GetBytes("author F\u00f6r fattare <a_u_thor@example.com> 1218123387 +0700\n"));
				b.Write(_eucJpEnc.GetBytes("committer C O. Miter <c@example.com> 1218123390 -0500\n"));
				b.Write(_eucJpEnc.GetBytes("encoding euc_JP\n"));
				b.Write(_eucJpEnc.GetBytes("\n"));
				b.Write(_eucJpEnc.GetBytes("\u304d\u308c\u3044\n"));
				b.Write(_eucJpEnc.GetBytes("\n"));
				b.Write(_eucJpEnc.GetBytes("Hi\n"));

				c = new RevCommit(Id("9473095c4cb2f12aefe1db8a355fe3fafba42f67")); // bogus id
				c.parseCanonical(new GitSharp.RevWalk.RevWalk(db), ((MemoryStream)b.BaseStream).ToArray());
			}
			Assert.Equal("F\u00f6r fattare", c.getAuthorIdent().Name);
			Assert.Equal("\u304d\u308c\u3044", c.getShortMessage());
			Assert.Equal("\u304d\u308c\u3044\n\nHi\n", c.getFullMessage());
		}

		/// <summary>
		/// This is a twisted case, but show what we expect here. We can revise the
		/// expectations provided this case is updated.
		/// 
		/// What happens here is that an encoding us given, but data is not encoded
		/// that way (and we can detect it), so we try other encodings.
		/// </summary>
		[StrictFactAttribute]
		public void testParse_explicit_bad_encoded()
		{
			RevCommit c;
			using (var b = new BinaryWriter(new MemoryStream()))
			{
				b.Write(_utf8Enc.GetBytes("tree 9788669ad918b6fcce64af8882fc9a81cb6aba67\n"));
				b.Write(_isoEnc.GetBytes("author F\u00f6r fattare <a_u_thor@example.com> 1218123387 +0700\n"));
				b.Write(_utf8Enc.GetBytes("committer C O. Miter <c@example.com> 1218123390 -0500\n"));
				b.Write(_utf8Enc.GetBytes("encoding EUC-JP\n"));
				b.Write(_utf8Enc.GetBytes("\n"));
				b.Write(_utf8Enc.GetBytes("\u304d\u308c\u3044\n"));
				b.Write(_utf8Enc.GetBytes("\n"));
				b.Write(_utf8Enc.GetBytes("Hi\n"));

				c = new RevCommit(Id("9473095c4cb2f12aefe1db8a355fe3fafba42f67")); // bogus id
				c.parseCanonical(new GitSharp.RevWalk.RevWalk(db), ((MemoryStream)b.BaseStream).ToArray());
			}
			Assert.Equal("F\u00f6r fattare", c.getAuthorIdent().Name);
			Assert.Equal("\u304d\u308c\u3044", c.getShortMessage());
			Assert.Equal("\u304d\u308c\u3044\n\nHi\n", c.getFullMessage());
		}

		/// <summary>
		/// This is a twisted case too, but show what we expect here. We can revise the
		/// expectations provided this case is updated.
		/// 
		/// What happens here is that an encoding us given, but data is not encoded
		/// that way (and we can detect it), so we try other encodings. Here data could
		/// actually be decoded in the stated encoding, but we override using UTF-8.
		/// </summary>
		[StrictFactAttribute]
		public void testParse_explicit_bad_encoded2()
		{
			RevCommit c;
			using (var b = new BinaryWriter(new MemoryStream()))
			{
				b.Write(_utf8Enc.GetBytes("tree 9788669ad918b6fcce64af8882fc9a81cb6aba67\n"));
				b.Write(_utf8Enc.GetBytes("author F\u00f6r fattare <a_u_thor@example.com> 1218123387 +0700\n"));
				b.Write(_utf8Enc.GetBytes("committer C O. Miter <c@example.com> 1218123390 -0500\n"));
				b.Write(_utf8Enc.GetBytes("encoding ISO-8859-1\n"));
				b.Write(_utf8Enc.GetBytes("\n"));
				b.Write(_utf8Enc.GetBytes("\u304d\u308c\u3044\n"));
				b.Write(_utf8Enc.GetBytes("\n"));
				b.Write(_utf8Enc.GetBytes("Hi\n"));

				c = new RevCommit(Id("9473095c4cb2f12aefe1db8a355fe3fafba42f67")); // bogus id
				c.parseCanonical(new GitSharp.RevWalk.RevWalk(db), ((MemoryStream)b.BaseStream).ToArray());
			}

			Assert.Equal("F\u00f6r fattare", c.getAuthorIdent().Name);
			Assert.Equal("\u304d\u308c\u3044", c.getShortMessage());
			Assert.Equal("\u304d\u308c\u3044\n\nHi\n", c.getFullMessage());
		}

		[StrictFactAttribute]
		public void testParse_NoMessage()
		{
			string msg = string.Empty;
			RevCommit c = Create(msg);
			Assert.Equal(msg, c.getFullMessage());
			Assert.Equal(msg, c.getShortMessage());
		}

		[StrictFactAttribute]
		public void testParse_OnlyLFMessage()
		{
			RevCommit c = Create("\n");
			Assert.Equal("\n", c.getFullMessage());
			Assert.Equal(string.Empty, c.getShortMessage());
		}

		[StrictFactAttribute]
		public void testParse_ShortLineOnlyNoLF()
		{
			const string shortMsg = "This is a short message.";
			RevCommit c = Create(shortMsg);
			Assert.Equal(shortMsg, c.getFullMessage());
			Assert.Equal(shortMsg, c.getShortMessage());
		}

		[StrictFactAttribute]
		public void testParse_ShortLineOnlyEndLF()
		{
			const string shortMsg = "This is a short message.";
			const string fullMsg = shortMsg + "\n";
			RevCommit c = Create(fullMsg);
			Assert.Equal(fullMsg, c.getFullMessage());
			Assert.Equal(shortMsg, c.getShortMessage());
		}

		[StrictFactAttribute]
		public void testParse_ShortLineOnlyEmbeddedLF()
		{
			const string fullMsg = "This is a\nshort message.";
			string shortMsg = fullMsg.Replace('\n', ' ');
			RevCommit c = Create(fullMsg);
			Assert.Equal(fullMsg, c.getFullMessage());
			Assert.Equal(shortMsg, c.getShortMessage());
		}

		[StrictFactAttribute]
		public void testParse_ShortLineOnlyEmbeddedAndEndingLF()
		{
			const string fullMsg = "This is a\nshort message.\n";
			const string shortMsg = "This is a short message.";
			RevCommit c = Create(fullMsg);
			Assert.Equal(fullMsg, c.getFullMessage());
			Assert.Equal(shortMsg, c.getShortMessage());
		}

		[StrictFactAttribute]
		public void testParse_GitStyleMessage()
		{
			const string shortMsg = "This fixes a bug.";
			const string body = "We do it with magic and pixie dust and stuff.\n"
								+ "\n" + "Signed-off-by: A U. Thor <author@example.com>\n";
			const string fullMsg = shortMsg + "\n" + "\n" + body;
			RevCommit c = Create(fullMsg);
			Assert.Equal(fullMsg, c.getFullMessage());
			Assert.Equal(shortMsg, c.getShortMessage());
		}

		private static ObjectId Id(string str)
		{
			return ObjectId.FromString(str);
		}
	}
}
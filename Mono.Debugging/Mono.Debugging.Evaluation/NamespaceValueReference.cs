// NamespaceValueReference.cs
//
// Author:
//   Lluis Sanchez Gual <lluis@novell.com>
//
// Copyright (c) 2008 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//

using System;
using System.Collections.Generic;

using Mono.Debugging.Client;

namespace Mono.Debugging.Evaluation
{
	public class NamespaceValueReference: ValueReference
	{
		readonly string namspace;
		readonly string name;

		public NamespaceValueReference (EvaluationContext ctx, string name) : base (ctx)
		{
			this.namspace = name;
			int i = namspace.LastIndexOf ('.');
			if (i != -1)
				this.name = namspace.Substring (i+1);
			else
				this.name = namspace;
		}

		public override object Value {
			get {
				throw new NotSupportedException();
			}
			set {
				throw new NotSupportedException();
			}
		}

		
		public override object Type {
			get {
				throw new NotSupportedException();
			}
		}

		
		public override object ObjectValue {
			get {
				throw new NotSupportedException ();
			}
		}

		
		public override string Name {
			get {
				return name;
			}
		}

		
		public override ObjectValueFlags Flags {
			get {
				return ObjectValueFlags.Namespace;
			}
		}

		public override ValueReference GetChild (string name, EvaluationOptions options)
		{
			string newNs = namspace + "." + name;
			
			EvaluationContext ctx = GetContext (options);
			object t = ctx.Adapter.GetType (ctx, newNs);
			if (t != null)
				return new TypeValueReference (ctx, t);
			
			return new NamespaceValueReference (ctx, newNs);
		}

		public override ObjectValue[] GetChildren (ObjectPath path, int index, int count, EvaluationOptions options)
		{
			List<ObjectValue> obs = new List<ObjectValue> ();
			foreach (ValueReference val in GetChildReferences (options)) {
				obs.Add (val.CreateObjectValue (options));
			}
			return obs.ToArray ();
		}

		public override IEnumerable<ValueReference> GetChildReferences (EvaluationOptions options)
		{
			// Child types

			string[] childNamespaces;
			string[] childTypes;

			EvaluationContext ctx = GetContext (options);
			ctx.Adapter.GetNamespaceContents (ctx, namspace, out childNamespaces, out childTypes);

			List<ValueReference> list = new List<ValueReference> ();
			foreach (string typeName in childTypes) {
				object tt = ctx.Adapter.GetType (ctx, typeName);
				if (tt != null)
					list.Add (new TypeValueReference (ctx, tt));
			}
			list.Sort (delegate (ValueReference v1, ValueReference v2) {
				return v1.Name.CompareTo (v2.Name);
			});
			
			// Child namespaces
			
			List<ValueReference> listNs = new List<ValueReference> ();
			foreach (string ns in childNamespaces)
				listNs.Add (new NamespaceValueReference (ctx, ns));
			listNs.Sort (delegate (ValueReference v1, ValueReference v2) {
				return v1.Name.CompareTo (v2.Name);
			});
			list.AddRange (listNs);
			return list;
		}

		protected override Mono.Debugging.Client.ObjectValue OnCreateObjectValue (EvaluationOptions options)
		{
			return Mono.Debugging.Client.ObjectValue.CreateObject (this, new ObjectPath (Name), "<namespace>", namspace, Flags, null);
		}

		public override string CallToString ()
		{
			return namspace;
		}
	}
}

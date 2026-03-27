# Test Cases — Item Processor Application

## Module 1: Login & Authentication

---

### TC-01 — Successful login with valid credentials
**Precondition:** User account exists in the database  
**Steps:**
1. Navigate to http://localhost:5000
2. Enter valid email: admin@test.com
3. Enter valid password: Admin@123
4. Click "Sign in"

**Expected Result:** User is redirected to Items page. Navbar shows logout option.  
**Type:** Positive

---

### TC-02 — Login with wrong password
**Precondition:** User account exists  
**Steps:**
1. Enter valid email: admin@test.com
2. Enter wrong password: wrongpass
3. Click "Sign in"

**Expected Result:** Error message "Invalid email or password" shown. User stays on login page.  
**Type:** Negative / Edge Case

---

### TC-03 — Login with non-existent email
**Steps:**
1. Enter email: nobody@fake.com
2. Enter any password
3. Click "Sign in"

**Expected Result:** Same generic error "Invalid email or password" — does NOT reveal whether email exists (security).  
**Type:** Negative / Security Edge Case

---

### TC-04 — Login with empty fields
**Steps:**
1. Leave email and password blank
2. Click "Sign in"

**Expected Result:** Validation messages appear: "Email is required", "Password is required". Form not submitted.  
**Type:** Edge Case / Validation

---

### TC-05 — Login with invalid email format
**Steps:**
1. Enter email: notanemail
2. Enter password: anything
3. Click "Sign in"

**Expected Result:** Validation message "Invalid email format". Form not submitted.  
**Type:** Edge Case / Validation

---

### TC-06 — Accessing protected page without login
**Steps:**
1. Clear browser cookies / open incognito
2. Navigate directly to http://localhost:5000/Item

**Expected Result:** User is redirected to /Account/Login. Cannot access items without authentication.  
**Type:** Security / Edge Case

---

### TC-07 — Successful logout
**Precondition:** User is logged in  
**Steps:**
1. Click "Logout" in the navbar
2. Try navigating back to /Item

**Expected Result:** User is logged out and redirected to Login page. Navigating to /Item redirects to Login again.  
**Type:** Positive

---

## Module 2: Item Screen (Add, Update, Delete, List, Search)

---

### TC-08 — Add a new item with valid data
**Precondition:** User is logged in  
**Steps:**
1. Navigate to Items page
2. Click "+ Add Item"
3. Enter Name: "Test Item A"
4. Enter Weight: 50.000
5. Enter Description: "A test item"
6. Click "Save Item"

**Expected Result:** Success message shown. New item appears in the items list with correct data.  
**Type:** Positive

---

### TC-09 — Add item with empty name
**Steps:**
1. Click "+ Add Item"
2. Leave Name blank
3. Enter Weight: 10.000
4. Click "Save Item"

**Expected Result:** Validation error "Item name is required". Item not saved.  
**Type:** Negative / Validation

---

### TC-10 — Add item with zero weight
**Steps:**
1. Click "+ Add Item"
2. Enter Name: "Zero Weight Item"
3. Enter Weight: 0
4. Click "Save Item"

**Expected Result:** Validation error "Weight must be greater than 0". Item not saved.  
**Type:** Edge Case / Validation

---

### TC-11 — Add item with negative weight
**Steps:**
1. Enter Name: "Negative Item"
2. Enter Weight: -5.000
3. Click "Save Item"

**Expected Result:** Validation error shown. Item not saved.  
**Type:** Edge Case / Validation

---

### TC-12 — Edit an existing item
**Precondition:** At least one item exists  
**Steps:**
1. Click "Edit" on any item
2. Change Name to "Updated Item Name"
3. Change Weight to 75.000
4. Click "Update"

**Expected Result:** Success message shown. Item list shows updated name and weight.  
**Type:** Positive

---

### TC-13 — Delete an item (soft delete)
**Precondition:** At least one item exists  
**Steps:**
1. Click "Delete" on any item
2. Confirm the dialog

**Expected Result:** Item disappears from the list. Success message shown. In DBeaver, the item still exists but IsActive = 0.  
**Type:** Positive

---

### TC-14 — Search items by name
**Precondition:** Items exist with names containing "Raw"  
**Steps:**
1. Type "Raw" in the search box
2. Click "Search"

**Expected Result:** Only items with "Raw" in their name are shown. Other items hidden.  
**Type:** Positive

---

### TC-15 — Search with no matching results
**Steps:**
1. Type "XYZNOTEXIST" in search box
2. Click "Search"

**Expected Result:** "No items found" message displayed. No error thrown.  
**Type:** Edge Case

---

### TC-16 — Search with empty string (clear search)
**Steps:**
1. Type something, search
2. Clear the search box
3. Click "Clear" button

**Expected Result:** All items are shown again.  
**Type:** Edge Case

---

## Module 3: Process Item

---

### TC-17 — Process an item with valid single output
**Precondition:** At least one item exists  
**Steps:**
1. Click "Process" on "Raw Material A" (100kg)
2. Add output: Name "Output X", Weight 45.000
3. Click "Process Item"

**Expected Result:** Success message. New item "Output X" appears in Items list. Processing record appears in Processed Items.  
**Type:** Positive

---

### TC-18 — Process an item with multiple outputs
**Steps:**
1. Click "Process" on an item
2. Add output 1: Name "Part A", Weight 30.000
3. Click "+ Add Output"
4. Add output 2: Name "Part B", Weight 20.000
5. Click "Process Item"

**Expected Result:** Both output items created. Both appear in processed items list linked to same parent.  
**Type:** Positive

---

### TC-19 — Output weight exceeds parent weight
**Steps:**
1. Select parent item with Weight 50.000
2. Add output: Weight 60.000
3. Click "Process Item"

**Expected Result:** Error "Total output weight (60.000) cannot exceed parent weight (50.000)". Nothing saved.  
**Type:** Edge Case / Validation — Most Critical

---

### TC-20 — Output weight exactly equals parent weight
**Steps:**
1. Select parent item with Weight 50.000
2. Add outputs that total exactly 50.000
3. Click "Process Item"

**Expected Result:** Processing succeeds. This is a valid boundary condition.  
**Type:** Edge Case / Boundary

---

### TC-21 — Process with no output items
**Steps:**
1. Select a parent item
2. Remove all output rows (click ✕ on the only row)
3. Try to submit

**Expected Result:** Cannot remove the last row — UI prevents it with alert "At least one output item is required."  
**Type:** Edge Case / Validation

---

### TC-22 — Process with empty output item name
**Steps:**
1. Select parent item
2. Leave output name blank
3. Enter weight 10.000
4. Click "Process Item"

**Expected Result:** Validation error "Child item name is required". Nothing saved.  
**Type:** Negative / Validation

---

### TC-23 — Process an item that was already a child (multi-level)
**Precondition:** "Output X" already exists as a child of "Raw Material A"  
**Steps:**
1. Click "Process" on "Output X"
2. Add output: Name "Sub Item", Weight 20.000
3. Click "Process Item"

**Expected Result:** Processing succeeds. Tree now shows 3 levels: Raw Material A → Output X → Sub Item.  
**Type:** Positive / Recursive

---

### TC-24 — Process with no parent selected
**Steps:**
1. Go to Process page
2. Leave parent dropdown at default "-- Select --"
3. Click "Process Item"

**Expected Result:** Validation error "Parent item is required". Nothing saved.  
**Type:** Negative / Validation

---

## Module 4: Tree View

---

### TC-25 — View tree for a root item
**Precondition:** Item has been processed with children  
**Steps:**
1. Click "Tree View" in navbar
2. Click "View Tree" for "Raw Material A"

**Expected Result:** Tree displays root item at level 0, children indented at level 1, grandchildren at level 2.  
**Type:** Positive

---

### TC-26 — Tree for unprocessed item
**Steps:**
1. Click "Tree" button on an item that has never been processed

**Expected Result:** Tree shows only the root item with no children. No error.  
**Type:** Edge Case

---

### TC-27 — Multi-level tree (3+ levels)
**Precondition:** A 3-level processing chain exists  
**Steps:**
1. View tree for the root item of a 3-level chain

**Expected Result:** All levels displayed with correct indentation and weight values at each node.  
**Type:** Positive / Deep Recursion

---

### TC-28 — Tree weight summary accuracy
**Steps:**
1. View tree for an item with known weights
2. Check the weight summary panel

**Expected Result:** Total output weight = sum of all direct children's output weights. Unaccounted = parent weight minus total output.  
**Type:** Positive / Data Accuracy
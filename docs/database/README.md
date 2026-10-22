# Frost database diagrams

- `Frost-ER.drawio`: editable diagrams.net file. Open https://app.diagrams.net, choose Device, then File > Open From > Device and select this file. Tables, individual attributes and connectors can all be edited. The file has three pages: Shop tables, Authentication tables, and Framework tables. It includes all 18 tables and 119 columns.
- `Frost-shop-ER.png`: plain export of the Shop tables page, using crow's foot notation. Suitable for attaching to the submission document.
- `Frost-full-schema.mmd`: alternative editable Mermaid ER source covering all 18 tables. Exact SQL data types appear in attribute comments.

The schema was compared with the supplied Laravel migrations and extracted from CREATE TABLE declarations in the demonstration database snapshot. No customer records, account values, tokens, or other row data are included.

The Shop tables page shows the two enforced foreign keys: `categories.id` to `items.category_id`, and `users.id` to `audit_events.actor_user_id`. One category can have zero or many items, but each item must have exactly one category. One user can have zero or many audit events; an audit event can have zero or one user because its actor column is nullable. Category deletion is restricted; deleting a user sets the audit actor to NULL.

OAuth and session columns are logical references without declared foreign key constraints; these are listed on the Authentication tables page and represented with dashed relationships in the Mermaid source. `oauth_clients.owner_type` and `owner_id` form an optional polymorphic owner reference. Audit `entity_type` and `entity_id` identify an item or customer without a database constraint, allowing history to survive record deletion. Customers are independent: the schema does not contain orders, sales, or customer-to-inventory relationships. The role is a users enum, so there is no separate roles table.

The refresh-token and device-code tables exist as framework schema, although this application enables only the authorization-code grant and disables refresh tokens. Tables are documented as implemented, including nullable timestamps and unique keys. `items.price >= 0` and `items.version >= 1`, `customers.version >= 1` are database check constraints. Inventory quantity and version fields use unsigned integers.

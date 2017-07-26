import * as React from 'react';
import { ConversationListItem } from '../../types/conversation';

export interface Props {
    list: ConversationListItem[],
    keywords: string,
    onSearch?: () => void;
    onMarkAsRead?: () => void;
}

export default function FilterList({ keywords, list, onSearch, onMarkAsRead }: Props) {
    return <div>
        <div><input value={keywords}></input><button onClick={onSearch}>Search</button></div>
        <div>
            <ul>
                {list.map(item => <li key={item.id}>{item.subject}</li>)}
            </ul>
        </div>
    </div>
}